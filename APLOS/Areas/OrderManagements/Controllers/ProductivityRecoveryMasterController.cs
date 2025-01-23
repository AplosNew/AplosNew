using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductivityRecoveryMasterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public ProductivityRecoveryMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpPost]
        public ActionResult GetBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MP.Id ManPowerBudgetId, MP.Code, E.UserName Entity, P.UserName Position,P.Activity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,DEG.UserName AS [LegalDesignation] from MST.ManpowerBudget MP
                            left join ORG.Entity E on E.Id = MP.EntityId
                            left join ORG.Position P on P.Id = MP.PositionId
							left join EmployeeInformation EI on EI.BudgetCode=MP.Id and EI.EmployeeStatus='Active'
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=P.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=P.SubSectionId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            where MP.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetUOM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select UM.Id UOMId, UM.Code,UM.StandardName, UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetUOMPF()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select UOMId as Value,(select UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1 and UM.Id=UOMPF.UOMId) as Text from [MST].[UOMProductionFactor] UOMPF";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPRMList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select BC.Code from [MST].[ManpowerBudget] BC where BC.Id=PRM.ResponsiblePersonBgtCodeId) as ResponsiblePersonBgtCode FROM [MST].[ProductivityRecoveryMaster] PRM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadUOMPFList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * ,(select UM.UserName from scs.UnitOfMeasurement UM where UM.Id=UPF.UOMId) as UOM FROM [MST].[UOMProductionFactor] UPF";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPRMEditData(string PRMID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select BC.Code from [MST].[ManpowerBudget] BC where BC.Id=PRM.ResponsiblePersonBgtCodeId) as ResponsiblePersonBgtCode FROM [MST].[ProductivityRecoveryMaster] PRM where PRM.Id='" + PRMID + @"'";
            return Json(new { prm = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadUOMPFEditData(string UOMPFID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * ,(select UM.UserName from scs.UnitOfMeasurement UM where UM.Id=UPF.UOMId) as UOM FROM [MST].[UOMProductionFactor] UPF where UPF.Id='" + UOMPFID + @"'";
            return Json(new { uom = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> PRMData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductivityRecoveryMaster] where FGProductGroup='" + PRMData["FGProductGroup"] + "'", out DataSet dsPRMFGProductGroupValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductivityRecoveryMaster] where StandardName='" + PRMData["StandardName"] + "'", out DataSet dsPRMStandardNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductivityRecoveryMaster] where UserName='" + PRMData["UserName"] + "'", out DataSet dsPRMUserNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductivityRecoveryMaster] where Code='" + PRMData["Code"] + "'", out DataSet dsPRMCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductivityRecoveryMaster] where RMGroup='" + PRMData["RMGroup"] + "'", out DataSet dsPRMRMGroupValidation, false, "1");

                DataSet dsProductivityRecoveryMaster;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductivityRecoveryMaster] where Id='" + PRMData["Id"] + "'", out dsProductivityRecoveryMaster, false, "1");
                string _Id = "";

                #region data update
                if (dsProductivityRecoveryMaster.Tables[0].Rows.Count == 0)
                {
                    if (dsPRMFGProductGroupValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("FG Product Group Already Exist.");
                    }
                    else if (dsPRMStandardNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsPRMUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else if (dsPRMCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Code Already Exist.");
                    }
                    else if (dsPRMRMGroupValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("RM Group Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("PRM", out _Id);
                        _Id = "PRM" + _Id;
                        PRMData["Id"] = _Id;
                        AddNewRow(dsProductivityRecoveryMaster.Tables[0], PRMData);
                    }
                }
                else
                {
                    _Id = PRMData["Id"].ToString();
                    EditRow(dsProductivityRecoveryMaster.Tables[0].Rows[0], PRMData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProductivityRecoveryMaster);

                return Json(new { Error = false, Data = PRMData, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public JsonResult CreateUOMPF(Dictionary<string, object> UOMData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[UOMProductionFactor] where UOMId='" + UOMData["UOMId"] + "'", out DataSet dsUOMProductionFactorUOMValidation, false, "1");
                
                DataSet dsUOMProductionFactor;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[UOMProductionFactor] where Id='" + UOMData["Id"] + "'", out dsUOMProductionFactor, false, "1");
                string _Id = "";

                #region data update
                if (dsUOMProductionFactor.Tables[0].Rows.Count == 0)
                {
                    if (dsUOMProductionFactorUOMValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("UOM Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("UPF", out _Id);
                        _Id = "UPF" + _Id;
                        UOMData["Id"] = _Id;
                        AddNewRow(dsUOMProductionFactor.Tables[0], UOMData);
                    }
                }
                else
                {
                    _Id = UOMData["Id"].ToString();
                    EditRow(dsUOMProductionFactor.Tables[0].Rows[0], UOMData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsUOMProductionFactor);

                return Json(new { Error = false, Data = UOMData, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize, HttpPost]
        public ActionResult PRMDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                
                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[ProductivityRecoveryMaster] where Id ='" + id + @"'");
                conC.CommitTransaction();
                
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult UOMPFDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();

                conC.BeginTransaction();
                conC.executeQuery("delete from [MST].[UOMProductionFactor] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadEntityDetails(string PRMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PRE.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PRE.Id,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code,PRE.Remarks,PRE.ResponsiblePersonId,
                            (select EmployeeName from EmployeeInformation where SystemId=PRE.ResponsiblePersonId) as ResponsiblePerson
                            from ORG.Entity E
							LEFT JOIN [TRN].[PRMEntity] PRE ON PRE.EntityId=E.Id and PRE.PRMId='" + PRMId + @"'
                            where E.Active = 1 order by PRE.EntityId  desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=P.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadFGArticleFilter()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MT.UserName MaterialType,MM.UserName Material,P.UserName Product,MC.UserName MaterialCategory,MSC.UserName MaterialSubCategory 
from MST.MaterialMasterArticle MA
left outer Join  MST.MaterialMaster MM ON MM.Id=MA.MaterialMasterId and MM.Active = 1 
left outer join MST.MaterialGroupMaster MG ON MG.Id=MM.MaterialGroupMasterId
left outer join HKP.MaterialType MT ON MT.Id=MG.MaterialTypeId
left outer join TRN.ProductDefinition PD ON PD.MaterialMasterId=MM.Id
left outer join MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
left outer join HKP.Product P ON P.Id=PM.ProductId
left outer join hkp.MaterialCategory MC ON MC.Id=MM.MaterialCategoryId
left outer join hkp.MaterialSubCategory MSC ON MSC.Id=MM.MaterialSubCategoryId";
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpGet]
        public ActionResult LoadFGArticleFilterRM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MT.UserName MaterialType,MM.UserName Material,P.UserName Product,MC.UserName MaterialCategory,MSC.UserName MaterialSubCategory 
from MST.MaterialMasterArticle MA
left outer Join  MST.MaterialMaster MM ON MM.Id=MA.MaterialMasterId and MM.Active = 1 
left outer join MST.MaterialGroupMaster MG ON MG.Id=MM.MaterialGroupMasterId
left outer join HKP.MaterialType MT ON MT.Id=MG.MaterialTypeId
left outer join TRN.ProductDefinition PD ON PD.MaterialMasterId=MM.Id
left outer join MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
left outer join HKP.Product P ON P.Id=PM.ProductId
left outer join hkp.MaterialCategory MC ON MC.Id=MM.MaterialCategoryId
left outer join hkp.MaterialSubCategory MSC ON MSC.Id=MM.MaterialSubCategoryId";
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult LoadFGArticleDetails(Dictionary<string, string> parameters, string PRMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PRA.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MA.Id as ArticleId,MA.StandardName Article,MM.Id MaterialMasterId,MM.UserName Material,
MT.UserName MaterialType,MG.UserName MaterialGroup,
AttributeValue=STUFF((SELECT distinct ', '+ A.UserName+ '-' + MAV.StandardName
                            FROM MST.MaterialMasterAttribute AS MMA
                            LEFT JOIN HKP.MaterialAttribute AS A ON MMA.MaterialAttributeId = A.Id
                            LEFT JOIN HKP.MaterialAttributeValue MAV ON MAV.MaterialAttributeId=MMA.MaterialAttributeId 
							LEFT JOIN MST.MaterialMasterArticleValue MMAV ON MAV.Id=MMAV.MaterialAttributeValueId
                            WHERE MMA.MaterialMasterId = MA.MaterialMasterId and MMAV.MaterialMasterArticleId=MA.Id for xml path('') ), 1, 1, '')
,PRA.Id,PRA.StdWorkingHours,isnull(convert(decimal(18,0),round((60/PRA.SPT)*UPF.ProductionFactor,1)),0) as StdProduction,PRA.IntermediateEfficiency,PRA.TargetEfficiency,PRA.Remarks,PRA.UtilizationPercentage,PRA.MachineSpeed,isnull(PRM.UserName,'') as PRMUserName,PRA.UOMId,(select UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1 and UM.Id=PRA.UOMId) as UOM,PRA.SPT
from MST.MaterialMasterArticle MA
left Join MST.MaterialMaster MM ON MM.Id=MA.MaterialMasterId and MM.Active = 1 
left outer join MST.MaterialGroupMaster MG ON MG.Id=MM.MaterialGroupMasterId
left outer join HKP.MaterialType MT ON MT.Id=MG.MaterialTypeId
left outer join TRN.ProductDefinition PD ON PD.MaterialMasterId=MM.Id
left outer join MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
left outer join HKP.Product P ON P.Id=PM.ProductId
left outer join hkp.MaterialCategory MC ON MC.Id=MM.MaterialCategoryId
left outer join hkp.MaterialSubCategory MSC ON MSC.Id=MM.MaterialSubCategoryId
LEFT JOIN [TRN].[PRMFGArticle] PRA ON PRA.ArticleId=MA.Id and PRA.PRMId='" + PRMId + @"'
left outer join MST.ProductivityRecoveryMaster PRM ON PRM.Id=PRA.PRMId
left outer join [MST].[UOMProductionFactor] UPF ON UPF.UOMId=PRA.UOMId
where MT.UserName IN(" + parameters["MaterialType"] + @") AND
      MM.UserName IN(" + parameters["Material"] + @") AND
      MC.UserName IN(" + parameters["MaterialCategory"] + @") AND
      (P.UserName IN (" + parameters["Product"] + @") or P.UserName is null) AND
      (MSC.UserName IN (" + parameters["MaterialSubCategory"] + @") or MSC.UserName is null) 
             order by PRA.Id  asc";
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet); 
            json.MaxJsonLength = int.MaxValue; 
            return json;
        }

        [Authorize, HttpGet]
        public ActionResult LoadProcessDetails(string PRMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select distinct CAST (CASE WHEN PRP.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,PRP.Id,P.Id ProcessId,P.UserName Process,PRP.Remarks,PRP.StdRecovery,PRP.TargetRecovery
                            from HKP.EntityProcessTag EP
							Left JOIN HKP.Process P ON P.Id=EP.ProcessId
							LEFT JOIN [TRN].[PRMProcess] PRP ON PRP.ProcessId=P.Id and PRP.PRMId='" + PRMId + @"'
                            where P.Active = 1 and EP.EntityId in (select EntityId from TRN.PRMEntity where PRMId='" + PRMId + @"')
							order by PRP.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult LoadRMArticleDetails(Dictionary<string, string> parametersRM, string PRMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN PRA.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MA.Id as ArticleId,MA.StandardName Article,MM.Id MaterialMasterId,MM.UserName Material,
MT.UserName MaterialType,MG.UserName MaterialGroup,
AttributeValue=STUFF((SELECT distinct ', '+ A.UserName+ '-' + MAV.StandardName
                            FROM MST.MaterialMasterAttribute AS MMA
                            LEFT JOIN HKP.MaterialAttribute AS A ON MMA.MaterialAttributeId = A.Id
                            LEFT JOIN HKP.MaterialAttributeValue MAV ON MAV.MaterialAttributeId=MMA.MaterialAttributeId 
							LEFT JOIN MST.MaterialMasterArticleValue MMAV ON MAV.Id=MMAV.MaterialAttributeValueId
                            WHERE MMA.MaterialMasterId = MA.MaterialMasterId and MMAV.MaterialMasterArticleId=MA.Id for xml path('') ), 1, 1, ''),
PRA.Id,PRA.StdProduction,PRA.IntermediateTarget,PRA.Remarks,(select UserName from HKP.CostingItem where Id=PRA.CostingItemId) as CostingItem
from MST.MaterialMasterArticle MA
left Join MST.MaterialMaster MM ON MM.Id=MA.MaterialMasterId and MM.Active = 1 
left outer join MST.MaterialGroupMaster MG ON MG.Id=MM.MaterialGroupMasterId
left outer join HKP.MaterialType MT ON MT.Id=MG.MaterialTypeId
left outer join TRN.ProductDefinition PD ON PD.MaterialMasterId=MM.Id
left outer join MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
left outer join HKP.Product P ON P.Id=PM.ProductId
left outer join hkp.MaterialCategory MC ON MC.Id=MM.MaterialCategoryId
left outer join hkp.MaterialSubCategory MSC ON MSC.Id=MM.MaterialSubCategoryId
LEFT JOIN [TRN].[PRMRMArticle] PRA ON PRA.ArticleId=MA.Id and PRA.PRMId='" + PRMId + @"'
where MT.UserName IN(" + parametersRM["MaterialType"] + @") AND
      MM.UserName IN(" + parametersRM["Material"] + @") AND
      MC.UserName IN(" + parametersRM["MaterialCategory"] + @") AND
      (P.UserName IN (" + parametersRM["Product"] + @") or P.UserName is null) AND
      (MSC.UserName IN (" + parametersRM["MaterialSubCategory"] + @") or MSC.UserName is null)
order by PRA.Id  asc";
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpPost]
        public ActionResult createEntity(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[PRMEntity]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PRMId='" + item["PRMId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PRE" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult createFGArticle(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked, dsArticleValidation;
            string TableName = "[TRN].[PRMFGArticle]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PRMId = '" + item["PRMId"] + "'", out dsProdBooked, false, "1");
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  ArticleId='" + item["ArticleId"] + "'", out dsArticleValidation, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            if (dsArticleValidation.Tables[0].Rows.Count > 0)
                            {
                                throw new Exception("This Article is Already Mapped");
                            }
                            else
                            { 
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PRA" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                            }
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult createProcess(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[PRMProcess]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PRMId='" + item["PRMId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "PRP" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult createRMArticle(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[PRMRMArticle]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PRMId='" + item["PRMId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "RMA" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
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

        [Authorize, HttpPost]
        public ActionResult GetCostingItem()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT CI.Id, CI.Code,CI.StandardName,CI.UserName,CI.Remarks
                            FROM HKP.CostingItem AS CI
                            WHERE CI.Active=1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadArticleMasterDetails(string PRMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select isnull(FGA.Id,'') as FGApplicableId,isnull(RMA.Id,'') as RMApplicableId,MA.Id as ArticleId,MA.StandardName Article,MM.Id MaterialMasterId,MM.UserName Material,
MT.UserName MaterialType,MG.UserName MaterialGroup,
AttributeValue=STUFF((select distinct ', '+ A.UserName+ '-' + MAV.StandardName from HKP.MaterialAttribute AS A  INNER JOIN HKP.MaterialAttributeValue MAV ON MAV.MaterialAttributeId=A.id left outer join MST.MaterialMasterAttribute MMA ON MAV.MaterialAttributeId=MMA.MaterialAttributeId where MMA.MaterialMasterId=MA.MaterialMasterId for xml path('') ), 1, 1, '')
,FGA.StdWorkingHours,FGA.StdProduction,FGA.IntermediateEfficiency,FGA.TargetEfficiency,FGA.Remarks,RMA.StdProduction as StdProductionRM,RMA.IntermediateTarget as IntermediateTargetRM,RMA.Remarks as RemarksRM,(select UserName from HKP.CostingItem where Id=RMA.CostingItemId) as CostingItem
from MST.MaterialMasterArticle MA
left outer Join MST.MaterialMaster MM ON MM.Id=MA.MaterialMasterId and MM.Active = 1 
left outer join MST.MaterialGroupMaster MG ON MG.Id=MM.MaterialGroupMasterId
left outer join HKP.MaterialType MT ON MT.Id=MG.MaterialTypeId
left outer join [TRN].[PRMFGArticle] FGA ON FGA.ArticleId=MA.Id and FGA.PRMId='" + PRMId + @"'
left outer join [TRN].[PRMRMArticle] RMA ON RMA.ArticleId=MA.Id and RMA.PRMId= '" + PRMId + @"' ";
            JsonResult json = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        #endregion -- Operations
    }
}