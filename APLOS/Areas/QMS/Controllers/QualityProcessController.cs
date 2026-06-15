#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.General.QMS;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
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

        #region Constructor
        QMSService qMSService = new QMSService();
        private readonly ISqlRepository _sqlRepository;
        public QualityProcessController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region Pages

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

        public ActionResult ImageMaster()
        {
            return View();
        }

        public ActionResult Inspection()
        {
            return View();
        }
        #endregion Pages

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
 Where FS.SalesOrderId='" + soId + "'";
            var colorItem = _sqlRepository.GetDataCollection(sql);
            sql = @"select distinct CV.Id ValueId,CV.UserName from TRN.SecondCharacteristics FS
LEFT JOIN HKP.CharacteristicsValue CV ON CV.Id=FS.CharacteristicsValueId 
 Where FS.SalesOrderId='" + soId + "'";

            var sizeItem = _sqlRepository.GetDataCollection(sql);
            return Json(new { colorItem, sizeItem }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeList(string column, string value, string plantId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (string.IsNullOrEmpty(plantId))
                {
                    plantId = identity.PlantId;
                }
                string CmdText = @"select top 500 * from (SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
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
                                        WHERE  EMP.EmployeeStatus='Active' AND EMP.EmpType<>'Guest' AND EMP.PlantId='" + plantId + @"'
UNION ALL
SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
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
                                        WHERE  EMP.EmployeeStatus='Active' AND EMP.EmpType<>'Guest' AND EMP.IsGlobalEmployee=1 
                                        ) AS TEMP WHERE " + strkey + " ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
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

        #region   Defect    
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

        [HttpPost, Authorize]
        public ActionResult GetDefectMarkerMasterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"select top 100 * from (SELECT DM.*,E.UserName Entity,W.UserName WorkCenterMaster,SD.UserName ProductionShift,CV.UserName Color,SV.UserName Size,ResponsiblePerson=(EI.EmployeeCode+'-'+ EI.EmployeeName)   
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
        public JsonResult SaveImageAndDefects(HttpPostedFileBase imageFile, string defectsJson, int masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                // Deserialize the payload
                var defectData = JsonConvert.DeserializeObject<DefectData>(defectsJson);

                string finalFileName = defectData.ImageFile;

                // ✅ If a new image is uploaded, save it
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string uploadsFolder = Path.Combine(ResourcesPathReader.GetDefectPicPath());
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    finalFileName = Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, finalFileName);
                    imageFile.SaveAs(filePath);
                }
                else
                {
                    // ✅ No new image uploaded — reuse existing file name
                    if (string.IsNullOrEmpty(finalFileName))
                        throw new Exception("No image provided and no existing file found.");
                }

                defectData.ImageFile = finalFileName;

                // Save defects in database
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                
                DataSet dsMaster;
                string tableName = "ImageDefects";

                con.OpenDataSetThroughAdapter($"SELECT * FROM {tableName} WHERE DefectMarkerMasterId=" + masterId + "", out dsMaster, false, "1");

                foreach (var d in defectData.Defects)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id=" + d.Id + " AND DefectMarkerMasterId='" + masterId + "'";


                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["ImageFile"] = defectData.ImageFile;
                        dr["DefectMarkerMasterId"] = masterId;
                        dr["Width"] = d.Width;
                        dr["Height"] = d.Height;
                        dr["XNormalized"] = d.XNormalized;
                        dr["YNormalized"] = d.YNormalized;
                        dr["Description"] = d.Description;
                        dr["DefectTypeId"] = d.DefectTypeId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dv[0].Row;

                        dr.BeginEdit();
                        dr["ImageFile"] = defectData.ImageFile;
                        dr["DefectMarkerMasterId"] = masterId;
                        dr["Width"] = d.Width;
                        dr["Height"] = d.Height;
                        dr["XNormalized"] = d.XNormalized;
                        dr["YNormalized"] = d.YNormalized;
                        dr["Description"] = d.Description;
                        dr["DefectTypeId"] = d.DefectTypeId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);
              

                

                return Json(new { Success = true, Message = "Image and defects saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetImageAndDefects(int masterId)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet ds = null;
                string sql = @"Select ID.*,DT.UserName Type from [dbo].[ImageDefects] ID
                LEFT JOIN HKP.DefectType DT ON DT.Id=ID.DefectTypeId WHERE DefectMarkerMasterId = " + masterId + "";
                con.OpenDataSetThroughAdapter(sql, out ds, false, "1");
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return Json(new { Success = false, Message = "No image found." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Assuming all defects share same image
                    var firstRow = ds.Tables[0].Rows[0];
                    string imageFile = Convert.ToString(firstRow["ImageFile"]);

                    var defects = ds.Tables[0].AsEnumerable().Select(r => new
                    {
                        Id = r["Id"],
                        XNormalized = r["XNormalized"],
                        YNormalized = r["YNormalized"],
                        Type = r["Type"],
                        Description = r["Description"]
                    });
                    return Json(new { Success = true, ImageFile = imageFile, Defects = defects }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion   Defect 

        #region ImageMaster

        [HttpPost, Authorize]
        public ActionResult GetImageMasterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM MST.ImageMaster ) AS TEMP WHERE " + strkey + " order by UserName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateImageMarkerMaster(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsCheckUserName;
                DataSet dsCheckStandardName;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from MST.ImageMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from MST.ImageMaster where Id<>'" + data["Id"] + "' AND UserName='" + data["UserName"] + "'", out dsCheckUserName, false, "1");
                con.OpenDataSetThroughAdapter("select * from MST.ImageMaster where Id<>'" + data["Id"] + "' AND StandardName='" + data["StandardName"] + "' ", out dsCheckStandardName, false, "1");

                string _Id = "";
                if (dsCheckUserName.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("UserName  " + data["UserName"] + " already have exist.!!");
                }
                if (dsCheckStandardName.Tables[0].Rows.Count > 0)
                {
                    throw new Exception(" StandardName " + data["StandardName"] + " already have exist.!!");
                }
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ImageMaster", out _Id);

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
        public JsonResult CreateImageMarkerEntity(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsEntity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [MST].[ImageEntity] where ImageMasterId='" + data["ImageMasterId"] + "' AND EntityId='" + data["EntityId"] + "'", out dsEntity, false, "1");

                string _Id = "";

                #region data update
                if (dsEntity.Tables[0].Rows.Count == 0)
                {
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID("ImageMaster", out _Id);

                    //data["Id"] = _Id;
                    AddNewRow(dsEntity.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsEntity.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEntity);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public JsonResult CreateImageMarkerProduct(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsEntity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from [MST].[ImageProduct] where ImageMasterId='" + data["ImageMasterId"] + "' AND ProductMasterId='" + data["ProductMasterId"] + "'", out dsEntity, false, "1");

                string _Id = "";

                #region data update
                if (dsEntity.Tables[0].Rows.Count == 0)
                {
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID("ImageMaster", out _Id);

                    //data["Id"] = _Id;
                    AddNewRow(dsEntity.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsEntity.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEntity);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost, Authorize]
        public ActionResult GetImageEntityList(string imageMasterId)
        {
            string sql = @"select IE.Id,E.UserName EntityName,IM.UserName ImageMaster,IE.ImageMasterId from [MST].[ImageEntity] IE 
                        LEFT JOIN ORG.Entity E ON E.Id=IE.EntityId
                        LEFT JOIN [MST].[ImageMaster] IM ON IM.Id=IE.ImageMasterId
                        WHERE ImageMasterId='" + imageMasterId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetImageProductMasterList(string imageMasterId)
        {

            string sql = @"select IP.Id,PM.UserName ProductName,IM.UserName ImageMaster,IP.ImageMasterId from [MST].[ImageProduct] IP 
                        LEFT JOIN [MST].[ProductMaster] PM ON PM.Id=IP.ProductMasterId
                        LEFT JOIN [MST].[ImageMaster] IM ON IM.Id=IP.ImageMasterId
                        WHERE ImageMasterId='" + imageMasterId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveImageArea(HttpPostedFileBase imageFile, string defectsJson, int masterId, List<Dictionary<string, object>> deletesData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                // Deserialize the payload
                var defectData = JsonConvert.DeserializeObject<ImageAreaData>(defectsJson);

                string finalFileName = defectData.ImageFile;

                // ✅ If a new image is uploaded, save it
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string uploadsFolder = Path.Combine(ResourcesPathReader.GetDefectPicPath());
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    finalFileName = Path.GetFileName(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, finalFileName);
                    imageFile.SaveAs(filePath);
                }
                else
                {
                    // ✅ No new image uploaded — reuse existing file name
                    if (string.IsNullOrEmpty(finalFileName))
                        throw new Exception("No image provided and no existing file found.");
                }

                defectData.ImageFile = finalFileName;

                // Save defects in database
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.BeginTransaction();
                DataSet dsMaster;
                string tableName = "dbo.ProductArea";

                con.OpenDataSetThroughAdapter($"SELECT * FROM {tableName} WHERE ImageMasterId=" + masterId + "", out dsMaster, false, "1");


                foreach (var d in defectData.ImageAreas)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id=" + d.Id + " AND ImageMasterId='" + masterId + "'";

                    //Random random = new Random();
                    //string randomNumber = random.Next(10000, 100000).ToString();
                    if (dv.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["ImageName"] = defectData.ImageFile;
                        dr["ImageID"] = masterId.ToString() + '-' + d.Code.ToString();
                        dr["ImageMasterId"] = masterId;
                        // dr["Width"] = d.Width;
                        // dr["Height"] = d.Height;
                        dr["XAxis"] = d.XAxis;
                        dr["YAxis"] = d.YAxis;
                        dr["AreaName"] = d.AreaName;
                        dr["Code"] = d.Code;
                        dr["Zone"] = d.Zone;
                        dr["Remarks"] = d.Remarks;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dv[0].Row;

                        dr.BeginEdit();
                        dr["ImageName"] = defectData.ImageFile;
                        dr["ImageID"] = masterId.ToString() + '-' + d.Code.ToString();
                        dr["ImageMasterId"] = masterId;
                        dr["XAxis"] = d.XAxis;
                        dr["YAxis"] = d.YAxis;
                        dr["AreaName"] = d.AreaName;
                        dr["Code"] = d.Code;
                        dr["Zone"] = d.Zone;
                        dr["Remarks"] = d.Remarks;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);
                foreach (var ditem in defectData.AreaDeleteData)
                {
                    con.executeQuery("delete from dbo.ProductArea where id='" + ditem.Id + "'");
                }
                con.CommitTransaction();
                return Json(new { Success = true, Message = "Image and Area saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message });
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetImageAreas(int masterId)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet ds = null;
                string sql = @"Select * from [dbo].[ProductArea]   WHERE ImageMasterId = " + masterId + "";
                con.OpenDataSetThroughAdapter(sql, out ds, false, "1");
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return Json(new { Success = false, Message = "No image found." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Assuming all defects share same image
                    var firstRow = ds.Tables[0].Rows[0];
                    string imageFile = Convert.ToString(firstRow["ImageName"]);

                    var defects = ds.Tables[0].AsEnumerable().Select(r => new
                    {
                        Id = r["Id"],
                        XAxis = r["XAxis"],
                        YAxis = r["YAxis"],
                        Code = r["Code"],
                        ImageName = r["ImageName"],
                        ImageID = r["ImageID"],
                        AreaName = r["AreaName"],
                        Zone = r["Zone"],
                        Remarks = r["Remarks"]
                    });
                    return Json(new { Success = true, ImageFile = imageFile, ImageAreas = defects }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region InspectionType
        public ActionResult ImageInspectionType()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetInspectionTypeList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM dbo.InspectionType ) AS TEMP WHERE " + strkey + " order by UserName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateImageInspectionType(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsCheckUserName;
                DataSet dsCheckStandardName;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.InspectionType where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.InspectionType where Id<>'" + data["Id"] + "' AND UserName='" + data["UserName"] + "'", out dsCheckUserName, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.InspectionType where Id<>'" + data["Id"] + "' AND StandardName='" + data["StandardName"] + "' ", out dsCheckStandardName, false, "1");

                string _Id = "";
                if (dsCheckUserName.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("UserName  " + data["UserName"] + " already have exist.!!");
                }
                if (dsCheckStandardName.Tables[0].Rows.Count > 0)
                {
                    throw new Exception(" StandardName " + data["StandardName"] + " already have exist.!!");
                }
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ImageMaster", out _Id);

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
        public JsonResult CreateImageInspectionTypeEntity(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsEntity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from InspectionEntity where InspectionTypeID='" + data["InspectionTypeID"] + "' AND EntityId='" + data["EntityId"] + "'", out dsEntity, false, "1");

                string _Id = "";

                #region data update
                if (dsEntity.Tables[0].Rows.Count == 0)
                {
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID("ImageMaster", out _Id);

                    //data["Id"] = _Id;
                    AddNewRow(dsEntity.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsEntity.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEntity);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public JsonResult CreateImageInspectionTypeProcess(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsEntity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from InspectionTypeProcess where InspectionTypeId='" + data["InspectionTypeId"] + "' AND ProcessId='" + data["ProcessId"] + "'", out dsEntity, false, "1");

                string _Id = "";

                #region data update
                if (dsEntity.Tables[0].Rows.Count == 0)
                {
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    //genid.GenID("ImageMaster", out _Id);

                    //data["Id"] = _Id;
                    AddNewRow(dsEntity.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsEntity.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEntity);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public JsonResult CreateImageInspectionTypeEntryLevel(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsEntity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from InspectionTypeEnteryLevel where InspectionTypeId='" + data["InspectionTypeId"] + "' ", out dsEntity, false, "1");

                string _Id = "";

                #region data update
                if (dsEntity.Tables[0].Rows.Count == 0)
                {
                    AddNewRow(dsEntity.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsEntity.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEntity);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost, Authorize]
        public ActionResult GetInspectionTypeEntityList(string imageInspectionTypeId)
        {
            string sql = @"select IE.Id,E.UserName EntityName,IM.UserName ImageMaster,IE.ImageMasterId from [MST].[ImageEntity] IE 
                        LEFT JOIN ORG.Entity E ON E.Id=IE.EntityId
                        LEFT JOIN [MST].[ImageMaster] IM ON IM.Id=IE.ImageMasterId
                        WHERE ImageMasterId='" + imageInspectionTypeId + @"'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetInspectionTypeProcessList(string imageInspectionTypeId)
        {
            string sql = @"SELECT DISTINCT P.Id, ITP.InspectionTypeId, P.StandardName, P.UserName
								, P.IsProductionProcess, P.IsProcessRouting, P.IsLocked
								, P.IsAppApplicable, IsChecked, P.IsValueAdded
								, P.MaterialTypeId, MT.[Description] AS MaterialType
								, P.Remarks,TG.ProductionBookingLevel
								, P.Active, P.Archive, Convert(bit,0) AS Flag,IsInventory= CAST(CASE	WHEN P.IsLast=1 THEN 1 WHEN M.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT)
							FROM [HKP].[Process] AS P
							LEFT JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
							LEFT JOIN [HKP].[EntityProcessTag] TG ON TG.ProcessId=P.Id 
							LEFT JOIN [dbo].[EntityConfig] M ON M.ConsumptionProcessId=P.Id 
							JOIN InspectionTypeProcess ITP ON ITP.ProcessId=P.Id
							WHERE ITP.InspectionTypeId='" + imageInspectionTypeId + @"'  AND P.IsProductionProcess=1 ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetInspectionTypeEntryLevelList(string imageInspectionTypeId)
        {
            string sql = @"SELECT * FROM InspectionTypeEnteryLevel
							WHERE  InspectionTypeId='" + imageInspectionTypeId + @"' ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]

        public JsonResult GetProductionProcessList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetProductionProcessList(parameters, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        public GridModel GetProductionProcessList(GridParameter parameters, string companyGroupId, string CompanyId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT P.Id, P.CompanyGroupId, P.Code
								, P.[Sequence], P.ShortName, P.StandardName, P.UserName
								, P.IsProductionProcess, P.IsProcessRouting, P.IsLocked
								, P.IsAppApplicable, IsChecked, P.IsValueAdded
								, P.MaterialTypeId, MT.[Description] AS MaterialType
								, P.Remarks,TG.ProductionBookingLevel
								, P.Active, P.Archive, Convert(bit,0) AS Flag,IsInventory= CAST(CASE	WHEN P.IsLast=1 THEN 1 WHEN M.Id IS NOT NULL THEN 1 ELSE 0 END AS BIT)
							FROM [HKP].[Process] AS P
							LEFT JOIN HKP.MaterialType AS MT ON P.MaterialTypeId=MT.Id
							LEFT JOIN [HKP].[EntityProcessTag] TG ON TG.ProcessId=P.Id 
							LEFT JOIN [dbo].[EntityConfig] M ON M.ConsumptionProcessId=P.Id 
							WHERE P.CompanyGroupId='" + companyGroupId + @"' AND P.IsProductionProcess=1 AND P.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }



        #endregion

        #region Inspection

        [HttpPost]
        public ActionResult GetInspectionList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT I.*,E.UserName Entity,W.UserName WorkCenter,P.UserName Process,S.ShiftDefinationName,
EmployeeName=EM.EmployeeCode+'-'+EM.EmployeeName,WCIncharge=WE.EmployeeCode+'-'+WE.EmployeeName
,ReportingOfficer=ER.EmployeeCode+'-'+ER.EmployeeName
FROM TRN.Inspection I
LEFT JOIN ORG.Entity E ON E.Id=I.EntityId
LEFT JOIN SCS.WorkCenterMaster W ON W.ID=I.WorkCenterMasterId
LEFT JOIN HKP.Process P ON P.Id=I.ProcessId
LEFT JOIN dbo.ShiftDefination S ON S.SystemID=I.ShiftId
LEFT JOIN dbo.EmployeeInformation EM ON EM.SystemId=I.EmployeeId
LEFT JOIN dbo.EmployeeInformation WE ON WE.SystemId=I.WCInchargeId
LEFT JOIN dbo.EmployeeInformation ER ON ER.SystemId=ReportingOfficerId) AS TEMP WHERE " + strkey + " Order by AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateInspection(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TRN.Inspection where InspectionUserName='" + data["InspectionUserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Inspection User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from TRN.Inspection where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("Inspection", out _Id);

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

        public ActionResult DeleteInspection(string id)
        {

            try
            {

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TRN.Inspection where Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }


        #endregion

        //Hello Mizan

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
        public string DefectTypeId { get; set; }   // added defect type
        public int Width { get; set; }
        public int Height { get; set; }
        public string DefectMarkerMasterId { get; set; }
    }

    public class ImageMaster
    {
        public int Id { get; set; }
        public string ImageFile { get; set; }
        public string ImageID { get; set; }
        public decimal XAxis { get; set; }
        public decimal YAxis { get; set; }
        public string Remarks { get; set; }
        public string Zone { get; set; }
        public string AreaName { get; set; }
        public string Code { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ImageMasterId { get; set; }
    }

    public class ImageAreaData
    {
        public string ImageFile { get; set; }

        public ImageDimensions ImageDimensions { get; set; }

        public List<ImageArea> ImageAreas { get; set; }
        public List<ImageArea> AreaDeleteData { get; set; }
    }
    public class ImageArea
    {
        public long? Id { get; set; }
        public decimal XAxis { get; set; }
        public decimal YAxis { get; set; }
        public string Remarks { get; set; }
        public string Zone { get; set; }
        public string AreaName { get; set; }
        public string Code { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ImageMasterId { get; set; }
        public string ImageID { get; set; }
    }

}


