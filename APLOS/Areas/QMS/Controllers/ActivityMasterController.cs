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

namespace Aplos.Areas.QMS.Controllers
{
    public class ActivityMasterController : BaseController
    {
        string TableName = "hkp.QMSActivityMaster";
        string TableName1 = "hkp.QMSProcess";
        string TableName2 = "hkp.QMSDepartment";
        string TableName3 = "HKP.QMSSubLocation";
        string TableName4 = "HKP.QMSResponsiblePerson";
        string TableName5 = "HKP.QMSDocument";
        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ActivityMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM "+ TableName +"  "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetCboSOP()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [HKP].[SOPItem]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult Getcboqmsactivitycategorylist()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName AS Text FROM [hkp].[QMSActivityCategory]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult Getcbobusinessprocesstypelist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[businessprocesstype]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult Getcbobusinessprocesslist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[BusinessProcess]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult Getqualityactivitychecktypelist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[qualityactivitychecktype]"), JsonRequestBehavior.AllowGet);
        }

  

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.QMSActivityMaster where Id = '" + Id + "' ");


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
           

            string sql = @"select top 100 * from (select QAM.*,QAC.UserName as QMSACId,BP.Username as BPUsername from [hkp].[QMSActivityMaster] QAM left join  [HKP].[QMSActivityCategory] QAC 
                          on QAM.QMSActivityCategoryId=QAC.Id left join [HKP].[BusinessProcess] BP 
                          on QAM.BusinessProcessId=BP.Id) AS TEMP WHERE " + strkey + " order by Sequence ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;
                DataSet dsMaster2;
                DataSet dsMaster3;
                DataSet dsMaster4;
                DataSet dsMaster5;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (data["Id"] == null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where ShortName= '" + data["ShortName"] + "' ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' ", out dsMaster1, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName= '" + data["StandardName"] + "' ", out dsMaster2, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster1.Tables[0].Rows.Count > 0 && dsMaster2.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Short Name, User Name and Standard Name already exists!!!");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Short Name already exists!!!");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                        throw new Exception("User Name already exists!!!");
                    if (dsMaster2.Tables[0].Rows.Count > 0)
                        throw new Exception("Standard Name already exists!!!");
                }

                else
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where ShortName= '" + data["ShortName"] + "' ", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where ShortName= '" + data["ShortName"] + "' and Id='" + data["Id"] + "' ", out dsMaster1, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' ", out dsMaster2, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName= '" + data["UserName"] + "' and Id='" + data["Id"] + "' ", out dsMaster3, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName= '" + data["StandardName"] + "' ", out dsMaster4, false, "1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName= '" + data["StandardName"] + "' and Id='" + data["Id"] + "' ", out dsMaster5, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0 && dsMaster1.Tables[0].Rows.Count == 0)
                        throw new Exception("Short Name already exists!!!");

                    if (dsMaster2.Tables[0].Rows.Count > 0 && dsMaster3.Tables[0].Rows.Count == 0)
                        throw new Exception("User Name already exists!!!");

                    if (dsMaster4.Tables[0].Rows.Count > 0 && dsMaster5.Tables[0].Rows.Count == 0)
                        throw new Exception("Standard Name already exists!!!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "QAM" + _Id;
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
            string sql = @"select * from [HKP].[QMSActivityMaster] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where QMSActivityMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Process");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where QMSActivityMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Department");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName3 + " where QMSActivityMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete SubLocation");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName4 + " where QMSActivityMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Responsible Person");
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName5 + " where QMSActivityMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Document");
                    }
                }


                // ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
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

     
        //      *****************PROCESS TAB*******************

        [HttpPost, Authorize]
        public ActionResult LoadAllProcessTabForSelection(string QMSActivityMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from HKP.Process WHERE isnull(ID,'') not in (select isnull(ProcessId,'') from HKP.QMSProcess where QMSActivityMasterId='" + QMSActivityMasterId + @"')
                  order by Sequence";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #region Multiple Value DocumentPreparedBy selection 


        [HttpPost]
        public JsonResult SaveProcessTab(string QMSActivityMasterId, List<Dictionary<string, object>> ProcessTabData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from HKP.QMSProcess where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < ProcessTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("QMSProcess", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
           
                    dr["ProcessId"] = ProcessTabData[i]["Id"].ToString();
                    
                    dr["QMSActivityMasterId"] = QMSActivityMasterId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Process updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult LoadAllSelectedProcessTab(string QMSActivityMasterId)
        {
            string sql = @" select qmsp.*,p.Sequence,p.Code,p.UserName,p.StandardName,p.ShortName from HKP.QMSProcess qmsp left join HKP.Process p
                            on qmsp.ProcessId=p.Id
							WHERE qmsp.QMSActivityMasterId='" + QMSActivityMasterId + @"' ";
              

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSelectedProcessTab(string Id)
        {
            try
            {
                string sql = @" delete from HKP.QMSProcess where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Process deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentPreparedBy selection 

        //      *****************Process TAB End*******************


        //      *****************DEPARTMENT TAB*******************
        [HttpPost, Authorize]
        public ActionResult LoadAllDepartmentTabForSelection(string QMSActivityMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from ORG.Department WHERE isnull(ID,'') not in (select isnull(DepartmentId,'') from HKP.QMSDepartment where QMSActivityMasterId='" + QMSActivityMasterId + @"')
                  order by Sequence";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #region Multiple Value DocumentPreparedBy selection 


        [HttpPost]
        public JsonResult SaveDepartmentTab(string QMSActivityMasterId, List<Dictionary<string, object>> DepartmentTabData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from HKP.QMSDepartment where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < DepartmentTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("QMSDepartment", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();

                    dr["DepartmentId"] = DepartmentTabData[i]["Id"].ToString();

                    dr["QMSActivityMasterId"] = QMSActivityMasterId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Department updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult LoadAllSelectedDepartmentTab(string QMSActivityMasterId)
        {
            string sql = @" select qmsdept.*,d.Sequence,d.Code,d.UserName,d.StandardName,d.ShortName from HKP.QMSDepartment qmsdept left join ORG.Department d
                            on qmsdept.DepartmentId=d.Id
							WHERE qmsdept.QMSActivityMasterId='" + QMSActivityMasterId + @"' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSelectedDepartmentTab(string Id)
        {
            try
            {
                string sql = @" delete from HKP.QMSDepartment where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Department deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentPreparedBy selection 


        //      *****************DEPARTMENT TAB END*******************


        //      *****************SUB LOCATION TAB*******************


        [HttpPost, Authorize]
        public ActionResult LoadAllSubLocationTabForSelection(string QMSActivityMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from (select distinct DSL.Id,DL.Id as DocLocId,DSL.Code as SubLocationCode,DSL.UserName as DocumentSubLocation,DL.Code as DocLocationCode,DL.UserName as DocumentLocation
                          from HKP.DocumentSubLocation DSL
                          inner join HKP.DocumentLocation DL ON DL.Id=DSL.DocumentLocationId
                          inner join dbo.SOPDocumentLocation SOPDL ON SOPDL.DocumentLocationId=DL.Id
                          where DSL.DocumentLocationId IN (
                          Select distinct SOPDL.DocumentLocationId from HKP.QMSDocument QD
                       
                          left join HKP.SOPDocument SD ON SD.Id=QD.DocumentId
                          left join dbo.SOPDocumentLocation SOPDL ON SOPDL.SOPDocumentId=QD.DocumentId
                          where QD.QMSActivityMasterId='" + QMSActivityMasterId + @"')
						  ) A
                          WHERE isnull(A.Id,'') not in (select isnull(SubLocationId,'') from HKP.QMSSubLocation where QMSActivityMasterId='" + QMSActivityMasterId + @"')
                  order by DocLocationCode";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #region Multiple Value DocumentPreparedBy selection 


        [HttpPost]
        public JsonResult SaveSubLocationTab(string QMSActivityMasterId, List<Dictionary<string, object>> SubLocationTabData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from HKP.QMSSubLocation where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < SubLocationTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("QMSSubLocation", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();

                    dr["SubLocationId"] = SubLocationTabData[i]["Id"].ToString();

                    dr["QMSActivityMasterId"] = QMSActivityMasterId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Sub Location updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult LoadAllSelectedSubLocationTab(string QMSActivityMasterId)
        {
            string sql = @"select qmssl.*,dsl.Sequence as Sequence,dsl.Code as SubLocationCode ,dsl.ShortName as ShortName,dsl.StandardName as StandardName,dsl.UserName as DocumentSubLocation, dl.UserName as DocumentLocation,dl.Code as DocLocationCode
                           from HKP.QMSSubLocation qmssl left join HKP.DocumentSubLocation dsl on qmssl.SubLocationId=dsl.Id
                           left join HKP.DocumentLocation dl on dsl.DocumentLocationId=dl.Id
                           WHERE qmssl.QMSActivityMasterId='" + QMSActivityMasterId + "' order by DocLocationCode ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSubLocationTab(string Id)
        {
            try
            {
                string sql = @" delete from HKP.QMSSubLocation where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Sub Location deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentPreparedBy selection 

        //      *****************SUB LOCATION TAB END *******************

        //      *****************RESPONSIBLE PERSON TAB*******************

        [HttpPost, Authorize]
        public ActionResult LoadAllDocumentPreparedByForSelection(string QMSActivityMasterId, string EntryType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"
                        SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId,'') from HKP.QMSResponsiblePerson where QMSActivityMasterId='" + QMSActivityMasterId + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            //position
            if (EntryType.ToUpper() != "EMPLOYEE")
            {
                sql = @"SELECT distinct convert(bit,0) AS isSelected,PR.Id,PR.Code,  isnull(DEG.UserName,'') Designation,
                            '' AS EmployeeName,PR.UserName PositionName, 'Active' As EmployeeStatus,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            SS.UserName SubSection
                            FROM  ORG.Position PR 
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN HKP.Designation DEG ON PR.DesignationId=DEG.Id
    
                        WHERE PR.CompanyGroupId='" + identity.CompanyGroupId + @"'
                AND ISNULL(pr.Id,'') not in (select ISNULL(PositionId,'') from HKP.QMSResponsiblePerson where QMSActivityMasterId='" + QMSActivityMasterId + @"')
                order by Code";


            }

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #region Multiple Value DocumentPreparedBy selection 


        [HttpPost]
        public JsonResult SaveResponsiblePerson(string QMSActivityMasterId, string EntryType, List<Dictionary<string, object>> DocumentPreparedByData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from HKP.QMSResponsiblePerson where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < DocumentPreparedByData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("QMSResponsiblePerson", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();
                    if (EntryType.ToUpper() == "EMPLOYEE")
                    {
                        dr["EmployeeId"] = DocumentPreparedByData[i]["Id"].ToString();
                    }
                    else
                    {
                        dr["PositionId"] = DocumentPreparedByData[i]["Id"].ToString();
                    }
                    dr["QMSActivityMasterId"] = QMSActivityMasterId;
                    dr["EntryType"] = EntryType;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Responsible Person updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult LoadAllSelectedDocumentPreparedBy(string QMSActivityMasterId)  //LoadAllSelectedDepartment
        {
            string sql = @" SELECT DISTINCT qmsrp.EntryType AS EntryTypeName, EMP.EmployeeStatus, qmsrp.Id,EMP.EmployeeCode AS Code, EMP.EmployeeName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName, DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                            INNER JOIN HKP.QMSResponsiblePerson AS qmsrp ON qmsrp.EmployeeId=emp.SystemId
							WHERE qmsrp.QMSActivityMasterId='" + QMSActivityMasterId + @"'
                            UNION ALL 
                            
                          SELECT DISTINCT qmsrp.EntryType AS EntryTypeName,CASE WHEN PR.Active=1 then 'Active' else 'Inactive' end AS EmployeeStatus, qmsrp.Id,PR.Code, '' AS EmployeeName,isnull(DEG.UserName,'') Designation,
                            pr.UserName AS PositionName,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                            FROM  ORG.Position PR 
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN HKP.Designation DEG ON PR.DesignationId=DEG.Id
                            INNER JOIN HKP.QMSResponsiblePerson AS qmsrp ON qmsrp.PositionId=PR.Id
                            
                             WHERE qmsrp.QMSActivityMasterId='" + QMSActivityMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteSelectedDocumentPreparedBy(string Id)
        {
            try
            {
                string sql = @" delete from HKP.QMSResponsiblePerson where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Responsible Person deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentPreparedBy selection 

        //      ***************** RESPONSIBLE PERSON TAB END *******************


        //************ Document Tab *******************

        [HttpPost, Authorize]
        public ActionResult LoadAllDocumentTabForSelection(string QMSActivityMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select sopd.*,sopdc.UserName as DocumentCategory,sopdsc.UserName as DocumentSubCategory from HKP.SOPDocument sopd left join HKP.SOPDocumentCategory sopdc
                            on sopd.SOPDocumentCategoryId=sopdc.Id left join HKP.SOPDocumentSubCategory sopdsc
                            on sopd.SOPDocumentSubCategoryId=sopdsc.Id
                            WHERE isnull(sopd.ID,'') not in (select isnull(DocumentId,'') from HKP.QMSDocument where QMSActivityMasterId='" + QMSActivityMasterId + @"')
                  order by Sequence";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #region Multiple Value DocumentPreparedBy selection 


        [HttpPost]
        public JsonResult SaveDocumentTab(string QMSActivityMasterId, List<Dictionary<string, object>> DocumentTabData)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("select * from HKP.QMSDocument where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < DocumentTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("QMSDocument", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = id + (i + 1).ToString();

                    dr["DocumentId"] = DocumentTabData[i]["Id"].ToString();

                    dr["QMSActivityMasterId"] = QMSActivityMasterId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

                return Json(new
                {
                    Error = false,
                    Message = "Document updated successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult LoadAllSelectedDocumentTab(string QMSActivityMasterId)
        {
            string sql = @" select qmsd.*,sopd.id as sopdid, sopd.Sequence as Sequence,sopd.Code as Code,sopd.ShortName as ShortName,sopd.StandardName as StandardName, sopd.Username as UserName, sopdc.UserName as DocumentCategory,sopdsc.UserName as DocumentSubCategory
                           from HKP.QMSDocument qmsd left join HKP.SOPDocument sopd on qmsd.DocumentId=sopd.Id
                           left join HKP.SOPDocumentCategory sopdc on sopd.SOPDocumentCategoryId=sopdc.Id
                           left join HKP.SOPDocumentSubCategory sopdsc on sopd.SOPDocumentSubCategoryId=sopdsc.Id
                           WHERE qmsd.QMSActivityMasterId='" + QMSActivityMasterId + "' order by Sequence ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult DeleteDocumentTab(string Id)
        {
            try
            {
                string sql = @" delete from HKP.QMSDocument where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Document deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Multiple Value DocumentPreparedBy selection 

        //**************** Document Tab End **********************

    }
    public class QMSDocument : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string DocumentId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSActivityMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }

    public class QMSSubLocation : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string SubLocationId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSActivityMasterId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }


        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }


        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}