using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Setups.Controllers
{
    public class ServiceMasterController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IServiceMasterService _serviceMasterService;

        public ServiceMasterController(IServiceMasterService serviceMasterService, ISqlRepository R)
        {
            this._serviceMasterService = serviceMasterService;
            _sqlRepository = R;
        }

        #endregion -- Constructor

        #region Pages


        public ActionResult Aplos()
        {
            return View();
        }


        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string ids)
        {
            return Json(_serviceMasterService.Query(parameters, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetServiceMasterList(GridParameter parameters)
        {
            return Json(_serviceMasterService.QueryServiceMaster(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetHSNCodeByServiceGroupId(string groupId)
        {
            var sql = @"SELECT Code FROM HKP.HSNCode WHERE Id =(SELECT HSNCodeId FROM [HKP].[ServiceGroup] WHERE Id='" + groupId + "')";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_serviceMasterService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ServiceMaster serviceMaster)
        {
            _serviceMasterService.Insert(serviceMaster);
            return Json(new { ServiceMaster = serviceMaster, Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ServiceMaster serviceMaster)
        {
            _serviceMasterService.Update(serviceMaster);
            return Json(new { Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _serviceMasterService.Delete(id);
                return Json(new { Sequence = _serviceMasterService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations

        #region Service Control

        public ActionResult ServiceControl()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateServiceControlHeader(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[ServiceControl] where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _detaliId = null;
                string _Id = "";
                bplib.clsGenID genid = new bplib.clsGenID();

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenerateIDYearly(DateTime.Now.ToString(), "ServiceControl", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update 

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [MST].[ServiceControl]");
            if (dt.Rows.Count > 0)
                return OTSBD.clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
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
        public ActionResult DeleteServiceControl(string id)
        {
            string sqlChild = @"SELECT * FROM [MST].[ServiceControlServiceMaster] WHERE ServiceControlId = '" + id + "'";
            string sql = @"SELECT * FROM [MST].[ServiceControl] WHERE Id = '" + id + "'";
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[ServiceControlServiceMaster] WHERE ServiceControlId='" + id + "'");
                con.executeQuery("DELETE FROM [MST].[ServiceControl] WHERE Id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetServiceControlList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM [MST].[ServiceControl]) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetServiceMasterList(string serviceControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"SELECT  CheckBoxSelect=cast(CASE WHEN SC.ServiceMasterId<>'' THEN 1  ELSE 0 END as bit),
                                    SG.UserName AS ServiceGroup,ISNULL(SM.ServiceCategory,'') ServiceCategory,ISNULL(SM.ServiceSubCategory,'') ServiceSubCategory,SM.UserName ServiceMaster,SM.IsPO,SM.IsApproved,SC.BudgetLimit,SC.Id,SM.Id ServiceMasterId,SC.ServiceControlId
                                    FROM [HKP].[ServiceMaster] SM
									 LEFT JOIN [HKP].[ServiceGroup] AS SG ON SG.Id=SM.ServiceGroupId
									left join(select * from  [MST].[ServiceControlServiceMaster] where ServiceControlId='" + serviceControlId + @"') SC on SC.ServiceMasterId=SM.Id
                                    --where SM.CompanyId='" + identity.CompanyId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetServiceActionByList(string serviceControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when gla.Id is null then 0 else 1 end as bit),gla.Id,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,DeM.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        LEFT JOIN(SELECT * FROM  [MST].[ServiceControlActionBy] where ServiceControlId='" + serviceControlId + @"') gla on gla.ActionById=EMP.SystemId
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetServiceApprovedByList(string serviceControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT CheckBoxSelect=cast(case when gla.Id is null then 0 else 1 end as bit),gla.Id,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,DeM.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,SE.UserName Section,EMP.SectionId,SuS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation,isnull( L.UserName,'') Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,DeM.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,EJ.JobLcSystemID,FORMAT(EJ.EffectiveDate,'dd-MMM-yyyy')EffectiveDate
                                        ,C.UserName Company,AM.Address1,EMP.PresentAddress1,EMP.CellPhnNo,EC.UserName EmployeeCategory,LPM.PolicyName
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN ORG.Company C ON C.Id=EMP.CompanyId
                                        LEFT JOIN MST.AddressMaster AM ON AM.Id=C.AddressMasterId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        left join ORG.Section SE on SE.Id=PR.SectionId
										LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN MST.DesignationMasterLegalDesignation DML ON DML.LegalDesignationId = EMP.LegalDesignationId
										Left join  MST.DesignationMaster DeM on DeM.Id = DML.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=DeM.DesignationId
                                        left join [MST].[DesignationMaster] DM on DM.DesignationId=EMP.GivenDesignationId
										left join SCS.DesignationMasterConfiguration DMC on DMC.DesignationMasterId=DM.Id and DMC.PlantId=emp.PlantId                
										left join [dbo].[LeavePolicyMaster] LPM on LPM.SystemID=DMC.LeavePolicyMasterId and LPM.PlantID=emp.PlantID
                                        left join [HKP].[EmployeeCategory] EC on EC.Id=DM.EmployeeCategoryId
                                        LEFT JOIN dbo.EmpDateWiseJobLocation EJ ON EJ.EmpsystemId=EMP.SystemId
										 AND EJ.SystemId=(Select top(1) SystemId from dbo.EmpDateWiseJobLocation JB Where JB.EmpSystemID=EMP.SystemId Order by EffectiveDate desc)
                                        LEFT JOIN(SELECT * FROM  [MST].[ServiceControlApprovedBy] where ServiceControlId='" + serviceControlId + @"') gla on gla.ActionById=EMP.SystemId
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetServiceControlEntityList(string serviceControlId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = "";
                sql = @"SELECT  CheckBoxSelect=cast(CASE WHEN SC.EntityId<>'' THEN 1  ELSE 0 END as bit),SC.Id,SM.Id EntityId,CO.UserName Company,P.UserName Plant,D.UserName Division,SD.UserName SubDivision
                                    ,SM.UserName EntityName
                                    FROM [ORG].[Entity] SM
									left join [ORG].[Company] CO ON CO.Id=SM.CompanyId
									left join [ORG].[Plant] P ON P.Id=SM.PlantId
									left join [ORG].[Division] D ON D.Id=SM.DivisionId
									left join [ORG].[SubDivision] SD ON SD.Id=SM.SubDivisionId
									LEFT JOIN(SELECT * FROM  [MST].[ServiceControlEntity] WHERE ServiceControlId='" + serviceControlId + @"') SC on SC.EntityId=SM.Id
                                    --where SM.CompanyId='" + identity.CompanyId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult CreateServiceControlServiceMaster(List<Dictionary<string, object>> data, string serviceControlId, string TabName)
        {
            try
            {
                DataSet dsDr, dsCr;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string Id = "";
                #region data update

                con.OpenDataSetThroughAdapter("select * from [MST].[ServiceControlServiceMaster] where ServiceControlId='" + serviceControlId + "'", out dsDr, false, "1");
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ServiceControlServiceMaster", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsDr.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ServiceControlId"] = serviceControlId;

                        AddNewRow(dsDr.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false && item["Id"].ToString() != null)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsDr);

                #endregion data update 


                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateServiceControlActionBy(List<Dictionary<string, object>> data, string ServiceControlId)
        {
            try
            {
                DataSet dsAB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [MST].[ServiceControlActionBy] where ServiceControlId='" + ServiceControlId + "'", out dsAB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ServiceControlActionBy", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ActionById"] = item["SystemID"];
                        item["ServiceControlId"] = ServiceControlId;

                        AddNewRow(dsAB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsAB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateServiceControlApprovedBy(List<Dictionary<string, object>> data, string ServiceControlId)
        {
            try
            {
                DataSet dsAPB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[ServiceControlApprovedBy] where ServiceControlId='" + ServiceControlId + "'", out dsAPB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ServiceControlApprovedBy", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAPB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ApproveById"] = item["SystemID"];
                        item["ServiceControlId"] = ServiceControlId;

                        AddNewRow(dsAPB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsAPB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateServiceControlEntity(List<Dictionary<string, object>> data, string ServiceControlId)
        {
            try
            {
                DataSet dsAPB;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [MST].[ServiceControlEntity] where ServiceControlId='" + ServiceControlId + "'", out dsAPB, false, "1");

                string Id = "";
                #region data update
                foreach (var item in data)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ServiceControlEntity", out Id);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    DataView dv = new DataView(dsAPB.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = Id;
                        item["ServiceControlId"] = ServiceControlId;

                        AddNewRow(dsAPB.Tables[0], item);
                    }
                    else if (dv.Count > 0 && Convert.ToBoolean(item["CheckBoxSelect"].ToString()) == false)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dv[0].Row["Id"].ToString();
                        EditRow(drmo, item);
                    }
                }
                #endregion data update 

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsAPB);
                return Json(new { Error = false, Id = Id, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion

        #region Upload Data
        [HttpGet, Authorize]
        public ActionResult GetSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetSampleFileServiceMaster(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Service Master Data upload Sample File";

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }

        public IWorkbook GetSampleFileServiceMaster(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
        {
            #region declare
            clsReport objRpt = null;
            OTSBD.clsStaticInfo objStatic = null;
            objStatic = new OTSBD.clsStaticInfo();
            string OTConsiderOn = string.Empty;

            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = ru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];
                xlsRow = 1;

                #region ------------------Column Header------------------


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceMasterId"); int colServiceMasterId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DrControlId"); int colDrControlId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CrControlId"); int colCrControlId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PurchaseApplicable"); int colPurchaseApplicable = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalesApplicable"); int colSalesApplicable = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IndependentApplicable"); int colIndependentApplicable = xlsCol;
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
               
                xlsRow++;

                sheet1.Range[xlsRow, colPurchaseApplicable, xlsRow, colPurchaseApplicable].DataValidation.AllowType = ExcelDataType.Integer;
                sheet1.Range[xlsRow, colSalesApplicable, xlsRow, colSalesApplicable].DataValidation.AllowType = ExcelDataType.Integer;
                sheet1.Range[xlsRow, colIndependentApplicable, xlsRow, colIndependentApplicable].DataValidation.AllowType = ExcelDataType.Integer;

                #endregion ------------------Column Header------------------

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup

                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult ImportData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<UploadedDataViewModel> data = new List<UploadedDataViewModel>();

                var file = Request.Files["file"];

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
                    {

                    }
                    else
                        throw new CustomException(Resources.ExcelUploadError);
                }
                else
                {
                    throw new CustomException(Resources.ExcelUploadError);
                }
                string path = "";
                if (file != null)
                {
                    path = Path.Combine(ResourcesPathReader.GetAttendanceRawData(), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        file.SaveAs(path);
                    }
                    else
                    {
                        file.SaveAs(path);
                    }
                }
                FileInfo docFile;
                string exception = "\r\n";
                try
                {
                    try
                    {
                        string connString = string.Empty;
                        ExcelEngine excelEngine = null;
                        IApplication application = null;
                        IWorkbook workbook = null;

                        excelEngine = new ExcelEngine();
                        application = excelEngine.Excel;
                        workbook = excelEngine.Excel.Workbooks.Open(path);

                        DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                        DataSet dsExcel = new DataSet();
                        dsExcel.Tables.Add(dt);


                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            exception += "\r\nTrying to delete";
                            docFile.Delete();
                        }

                        if (dsExcel.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                            {
                                UploadedDataViewModel vm = new UploadedDataViewModel();

                                vm.ServiceMasterId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.DrControlId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                                vm.CrControlId = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                                vm.PurchaseApplicable = dsExcel.Tables[0].Rows[i][3].ToString().Trim();
                                vm.SalesApplicable = dsExcel.Tables[0].Rows[i][4].ToString().Trim();
                                vm.IndependentApplicable = dsExcel.Tables[0].Rows[i][5].ToString().Trim();

                                data.Add(vm);

                            }
                        }
                        else
                        {
                            throw new Exception("Please Select File");
                        }
                    }
                    catch (Exception ex)
                    {

                        docFile = new FileInfo(path);
                        if (docFile.Exists)
                        {
                            docFile.Delete();
                        }
                        throw (ex);
                    }

                }
                catch (Exception ex)
                {
                    //throw ex;
                }
                finally
                {
                }
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveUploadedData(List<Dictionary<string, object>> data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            bplib.clsGenID genid = new bplib.clsGenID();
            DataSet dsBC, dsDD;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[ServiceMasterGL] where 1=1", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            item["PurchaseApplicable"] = (item["PurchaseApplicable"].ToString() == "1") ? true : false;
                            item["SalesApplicable"] = (item["SalesApplicable"].ToString() == "1") ? true : false;
                            item["IndependentApplicable"] = (item["IndependentApplicable"].ToString() == "1") ? true : false;
                            item["DrControlId"] = (item["DrControlId"].ToString() == null) ? DBNull.Value.ToString() : item["DrControlId"].ToString();
                            item["CrControlId"] = (item["CrControlId"].ToString() == null) ? DBNull.Value.ToString() : item["CrControlId"].ToString();
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
                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsBC);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion
    }

    public class UploadedDataViewModel
    {
        public string ServiceMasterId { get; set; }
        public string DrControlId { get; set; }
        public string CrControlId { get; set; }
        public string PurchaseApplicable { get; set; }
        public string SalesApplicable { get; set; }
        public string IndependentApplicable { get; set; }

    }
}