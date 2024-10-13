using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Materials;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
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
        [HttpGet, Authorize]
        public ActionResult GetServiceMasterGL(string ServiceMasterId)
        {
            var sql = @"SELECT * FROM [HKP].[ServiceMasterGL] Where ServiceMasterId='"+ ServiceMasterId + "'";
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

        [HttpPost, Authorize]
        public JsonResult GetServicePopUpList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetServiceList(column, value, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        public IEnumerable<object>  GetServiceList(string column, string value, string companyId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        SELECT TOP 400 * FROM (SELECT ST.UserName ServiceType,SG.UserName ServiceGroup,SM.Id ServiceMasterId,SM.UserName ServiceName,GL.AccountCode GLGeneralInfoCode
						,GL.UserName GLGeneralInfoName,B.UserName BudgetName,A.UserName ActivityName,BM.GLGeneralInfoId
                        ,BMA.BudgetMasterId,BMA.ActivityId ,BM.RefNo,SMGL.DrControlId,A.IsOrderSpecific,A.ActivityOrderType,A.ValueOfDIstribution 
                        FROM  HKP.ServiceMaster SM  
                        LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
                        LEFT JOIN HKP.ServiceType ST ON ST.Id=SG.ServiceTypeId
                        JOIN HKP.ServiceMasterGL SMGL ON SMGL.ServiceMasterId=SM.Id
                        LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.Id=SMGL.DrControlId
                        LEFT JOIN MST.BudgetMaster BM ON BM.Id=BMA.BudgetMasterId
                        LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                        LEFT JOIN HKP.Activity A ON A.Id=BMA.ActivityId
                        LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId) AS TEMP WHERE " + strkey + " order by ServiceName ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
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

        [Authorize, HttpGet]
        public ActionResult GetControlDrlist(string tabName,string companyId)
        {
            try
            {
                var sql = "";
                if (tabName == "ControlDr")
                {
                    sql = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName 
									,BMA.Active,BMA.Id BudgetMasterActivityId
                                    FROM [MST].[BudgetMasterActivity] BMA
									 JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									 JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
									WHERE GLGI.Archive=0 AND GLGI.Active=1 AND  GLCI.CompanyId='" + companyId + @"' AND BMA.Active=1 AND BM.Active=1";
                }
                else
                {
                    sql = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName 
									,BMA.Active,BMA.Id BudgetMasterActivityId
                                    FROM [MST].[BudgetMasterActivity] BMA
									 JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									 JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
									WHERE GLGI.Archive=0 AND GLGI.Active=1 AND  GLCI.CompanyId='" + companyId + @"' AND BMA.Active=1 AND BM.Active=1";
                }

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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

        public DataTable GetServiceMasterGLData()
        {
            var cmdText = @"SELECT GL.*,SM.UserName ServiceMaster,SM.ServiceCategory,SM.ServiceSubCategory,SG.UserName ServiceGroup,H.Code HSNCode, A.UserName DrActivityName, CA.UserName CrActivityName 
FROM [HKP].[ServiceMasterGL] GL
LEFT JOIN HKP.ServiceMaster SM ON SM.Id=GL.ServiceMasterId
LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
LEFT JOIN HKP.HSNCode H ON H.Id=SM.HSNCodeId
LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.Id=GL.DrControlId
LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
LEFT JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
LEFT JOIN [MST].[BudgetMasterActivity] CBMA ON BMA.Id=GL.CrControlId
LEFT JOIN [MST].[BudgetMaster] AS CBM ON CBM.Id=CBMA.BudgetMasterId
LEFT JOIN [HKP].[Activity] CA ON A.Id=CBMA.ActivityId";
            return _sqlRepository.GetDataTable(cmdText);
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


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceMasterId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colServiceMasterId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DrControlId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11; int colDrControlId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CrControlId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11; int colCrControlId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PurchaseApplicable"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 19; int colPurchaseApplicable = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalesApplicable"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15; int colSalesApplicable = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IndependentApplicable"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 22; int colIndependentApplicable = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "IsAssetApplicable"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17; int colIsAssetAplicable = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceMaster"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 30; int colServiceMaster = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceCategory"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colServiceCategory = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceSubCategory"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colServiceSubCategory = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceGroup"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25; int colServiceGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "HSNCode"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colHSNCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "DrActivityName"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colDrControl = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "CrActivityName"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 40; int colCrControl = xlsCol; 
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

                DataTable dtData = GetServiceMasterGLData();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colServiceMasterId].Text = dtData.Rows[i]["ServiceMasterId"].ToString();
                    sheet1[xlsRow, colDrControlId].Text = dtData.Rows[i]["DrControlId"].ToString();
                    sheet1[xlsRow, colCrControlId].Text = dtData.Rows[i]["CrControlId"].ToString();
                    if (dtData.Rows[i]["PurchaseApplicable"].ToString() == "False")
                    {
                        sheet1[xlsRow, colPurchaseApplicable].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colPurchaseApplicable].Text = "1";
                    }
                    if (dtData.Rows[i]["SalesApplicable"].ToString() == "False")
                    {
                        sheet1[xlsRow, colSalesApplicable].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colSalesApplicable].Text = "1";
                    }
                    if (dtData.Rows[i]["IndependentApplicable"].ToString() == "False")
                    {
                        sheet1[xlsRow, colIndependentApplicable].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colIndependentApplicable].Text = "1";
                    }
                    if (dtData.Rows[i]["IsAssetApplicable"].ToString() == "False")
                    {
                        sheet1[xlsRow, colIsAssetAplicable].Text = "0";
                    }
                    else
                    {
                        sheet1[xlsRow, colIsAssetAplicable].Text = "1";
                    }
                    sheet1[xlsRow, colServiceMaster].Text = dtData.Rows[i]["ServiceMaster"].ToString();
                    sheet1[xlsRow, colServiceCategory].Text = dtData.Rows[i]["ServiceCategory"].ToString();
                    sheet1[xlsRow, colServiceSubCategory].Text = dtData.Rows[i]["ServiceSubCategory"].ToString();
                    sheet1[xlsRow, colServiceGroup].Text = dtData.Rows[i]["ServiceGroup"].ToString();
                    sheet1[xlsRow, colHSNCode].Text = dtData.Rows[i]["HSNCode"].ToString();
                    sheet1[xlsRow, colDrControl].Text = dtData.Rows[i]["DrActivityName"].ToString();
                    sheet1[xlsRow, colCrControl].Text = dtData.Rows[i]["CrActivityName"].ToString();

                    xlsRow++;
                }


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
                                vm.IsAssetApplicable = dsExcel.Tables[0].Rows[i][6].ToString().Trim();

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
            DataSet dsBC, dsDD;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                string  strSQL = "Delete FROM [HKP].[ServiceMasterGL]";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[ServiceMasterGL] where 1=2", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            if (item["DrControlId"] == null || item["DrControlId"] == "")
                            {
                                item["DrControlId"] = null;
                            }
                            if (item["CrControlId"] == null|| item["CrControlId"] == "")
                            {
                                item["CrControlId"] = null;
                            }

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


        [HttpPost, Authorize]
        public JsonResult SaveGLData(List<Dictionary<string, object>> data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC, dsDD;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[ServiceMasterGL] where 1=2", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            if (item["DrControlId"] == null || item["DrControlId"] == "")
                            {
                                item["DrControlId"] = null;
                            }
                            if (item["CrControlId"] == null || item["CrControlId"] == "")
                            {
                                item["CrControlId"] = null;
                            }

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

        [HttpGet, Authorize]
        public ActionResult GetTDSSampleFile(ReportFormat reportFormat)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = GetTDSSampleFileServiceMaster(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName);
            var reportFileName = "Service Master TDS Data upload Sample File";

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

        public DataTable GetServiceMasterTDSData()
        {
            var cmdText = @"SELECT TDS.*,SM.UserName ServiceMaster,SM.ServiceCategory,SM.ServiceSubCategory,SG.UserName ServiceGroup,H.Code HSNCode,TC.UserName TaxCode,TXC.UserName TaxCategory
FROM [HKP].[ServiceMasterTDS] TDS
LEFT JOIN HKP.ServiceMaster SM ON SM.Id=TDS.ServiceMasterId
LEFT JOIN MST.TaxCode TC ON TC.Id=TDS.TaxCodeId
LEFT JOIN [MST].[TaxCategory] TXC ON TXC.Id=TC.TaxCategoryId
LEFT JOIN HKP.ServiceGroup SG ON SG.Id=SM.ServiceGroupId
LEFT JOIN HKP.HSNCode H ON H.Id=SM.HSNCodeId";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetTDSSampleFileServiceMaster(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName)
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


                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceMasterId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colServiceMasterId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "TaxCodeId"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colDrControlId = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceMaster"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colServiceMaster = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceCategory"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 16; int colServiceCategory = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceSubCategory"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 20; int colServiceSubCategory = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ServiceGroup"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25; int colServiceGroup = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "HSNCode"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colHSNCode = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "TaxCategory"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colTaxC = xlsCol; xlsCol += 1;
                ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "TaxCode"); sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10; int colTaxCode = xlsCol; 
                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                xlsRow++;

                #endregion ------------------Column Header------------------
                DataTable dtData = GetServiceMasterTDSData();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    sheet1[xlsRow, colServiceMasterId].Text = dtData.Rows[i]["ServiceMasterId"].ToString();
                    sheet1[xlsRow, colDrControlId].Text = dtData.Rows[i]["TaxCodeId"].ToString(); 
                    sheet1[xlsRow, colServiceMaster].Text = dtData.Rows[i]["ServiceMaster"].ToString();
                    sheet1[xlsRow, colTaxC].Text = dtData.Rows[i]["TaxCategory"].ToString();
                    sheet1[xlsRow, colTaxCode].Text = dtData.Rows[i]["TaxCode"].ToString();
                    sheet1[xlsRow, colServiceCategory].Text = dtData.Rows[i]["ServiceCategory"].ToString();
                    sheet1[xlsRow, colServiceSubCategory].Text = dtData.Rows[i]["ServiceSubCategory"].ToString();
                    sheet1[xlsRow, colServiceGroup].Text = dtData.Rows[i]["ServiceGroup"].ToString();
                    sheet1[xlsRow, colHSNCode].Text = dtData.Rows[i]["HSNCode"].ToString();
                    xlsRow++;
                }
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
        public JsonResult ImportTDSData(FormCollection form)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<UploadedTDSDataViewModel> data = new List<UploadedTDSDataViewModel>();

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
                                UploadedTDSDataViewModel vm = new UploadedTDSDataViewModel();

                                vm.ServiceMasterId = dsExcel.Tables[0].Rows[i][0].ToString().Trim();
                                vm.TaxCodeId = dsExcel.Tables[0].Rows[i][1].ToString().Trim();

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
        public JsonResult SaveUploadedTDSData(List<Dictionary<string, object>> data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsBC;
            string _Id = string.Empty;
            try
            {
                #region Entity 
                objCon = new ConnectionManager.DAL.ConManager("1");

                string strSQL = "Delete FROM [HKP].[ServiceMasterTDS]";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[ServiceMasterTDS] where 1=2", out dsBC, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsBC.Tables[0]);
                        dv.RowFilter = "Id='" + Convert.ToInt64(item["Id"]) + "'";

                        if (dv.Count == 0)
                        {
                            
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
        public string IsAssetApplicable { get; set; }

    }

    public class UploadedTDSDataViewModel
    {
        public string ServiceMasterId { get; set; }
        public string TaxCodeId { get; set; }

    }
}