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
    public class ProductIntegrityAnalysisMasterController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public ProductIntegrityAnalysisMasterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public ActionResult LoadProductIntegrityAnalysisMasterList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select E.EmployeeName from EmployeeInformation E where E.SystemId=PIAM.ResponsiblePersonId) as ResponsiblePerson  FROM [MST].[ProductIntegrityAnalysisMaster] PIAM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadPIAMEditData(string PIAMID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,(select E.EmployeeName from EmployeeInformation E where E.SystemId=PIAM.ResponsiblePersonId) as ResponsiblePerson  FROM [MST].[ProductIntegrityAnalysisMaster] PIAM where PIAM.Id='" + PIAMID + @"'";
            return Json(new { PIAM = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from HKP.Process";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        
        [Authorize, HttpGet]
        public decimal GetItemAutoSequence(string PIAMId)
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(SNO),0) AS SNO FROM [TRN].[ProductIntegrityAnalysisItem] where PIAMID='" + PIAMId + "'");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["SNO"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetUOM()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select UM.Id UOMId, UM.Code,UM.StandardName, UM.UserName UOM from scs.UnitOfMeasurement UM where UM.Active = 1";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> PIAMData)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductIntegrityAnalysisMaster] where Code='" + PIAMData["Code"] + "'", out DataSet dsPIAMCodeValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductIntegrityAnalysisMaster] where StandardName='" + PIAMData["StandardName"] + "'", out DataSet dsPIAMSNameValidation, false, "1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductIntegrityAnalysisMaster] where UserName='" + PIAMData["UserName"] + "'", out DataSet dsPIAMUNameValidation, false, "1");
                

                DataSet dsPIAM;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductIntegrityAnalysisMaster] where Id='" + PIAMData["Id"] + "'", out dsPIAM, false, "1");
                string _Id = "";

                #region data update
                if (dsPIAM.Tables[0].Rows.Count == 0)
                {
                    if (dsPIAMCodeValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Code Already Exist.");
                    }
                    else if (dsPIAMSNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Standared Name Already Exist.");
                    }
                    else if (dsPIAMUNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("PIAM", out _Id);
                        _Id = "PIA" + _Id;
                        PIAMData["Id"] = _Id;
                        AddNewRow(dsPIAM.Tables[0], PIAMData);
                    }
                }
                else
                {
                    _Id = PIAMData["Id"].ToString();
                    EditRow(dsPIAM.Tables[0].Rows[0], PIAMData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsPIAM);

                return Json(new { Error = false, Data = PIAMData, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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

        [HttpPost]
        public ActionResult PIAMDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                DataSet ItemCount;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [MST].[ProductIntegrityAnalysisItem] where PIAMID ='" + id + "'", out ItemCount, false, "1");
                

                if (ItemCount.Tables[0].Rows.Count == 0)
                {

                    conC.BeginTransaction();
                    conC.executeQuery("delete from [MST].[ProductIntegrityAnalysisMaster] where Id ='" + id + @"'");
                    conC.CommitTransaction();
                }
                else
                {
                    throw new Exception("Transaction are Exists!");
                }
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult ItemDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[ProductIntegrityAnalysisItem] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, P.Id AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active' AND EI.EmpType<>'Guest'";
            JsonResult json = Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemEditData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,(select UserName from HKP.Process where Active=1 and Id=ProcessId) as Process,
(select UserName from scs.UnitOfMeasurement where Active=1 and Id=UOMId) as UOM
FROM [TRN].[ProductIntegrityAnalysisItem] where Id='" + ItemId + @"'";
            return Json(new { item = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadParameterEditData(string ParameterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select * from ProductItemParameterDetails where Id='" + ParameterId + @"'";
            return Json(new { Parameter = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }
        
       
        [Authorize, HttpGet]
        public ActionResult LoadItemDetails(string ProductId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT *,(select UserName from HKP.Process where Active=1 and Id=ProcessId) as Process,
(select UserName from scs.UnitOfMeasurement where Active=1 and Id=UOMId) as UOM
FROM [TRN].[ProductIntegrityAnalysisItem] where PIAMID ='" + ProductId + "' order by SNO";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult getParameterData(string ItemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM ProductItemParameterDetails where ItemId ='" + ItemId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
      
      
        [Authorize, HttpPost]
        public JsonResult CreateItem(Dictionary<string, object> ItemData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[ProductIntegrityAnalysisItem] where Id<>'" + ItemData["Id"] + "'", out DataSet dsProductIntegrityAnalysisItemValidation, false, "1");

                DataSet dsProductIntegrityAnalysisItem;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[ProductIntegrityAnalysisItem] where Id='" + ItemData["Id"] + "'", out dsProductIntegrityAnalysisItem, false, "1");
                string _Id = "";

                #region data update
                if (dsProductIntegrityAnalysisItem.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ProductIntegrityAnalysisItem", out _Id);
                    _Id = "PAI" + _Id;
                    ItemData["Id"] = _Id;
                    ItemData["PIAMID"] = Pid;
                    AddNewRow(dsProductIntegrityAnalysisItem.Tables[0], ItemData);
                }
                else
                {
                    _Id = ItemData["Id"].ToString();
                    ItemData["PIAMID"] = Pid;
                    EditRow(dsProductIntegrityAnalysisItem.Tables[0].Rows[0], ItemData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProductIntegrityAnalysisItem);

                return Json(new { Error = false, Data = ItemData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [Authorize, HttpPost]
        public JsonResult CreateParameter(Dictionary<string, object> ParameterData, string Pid)
        {
            try
            {

                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from ProductItemParameterDetails where Id<>'" + ParameterData["Id"] + "'", out DataSet dsItemParameterDetailsValidation, false, "1");

                DataSet dsProductItemParameterDetails;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from ProductItemParameterDetails where Id='" + ParameterData["Id"] + "'", out dsProductItemParameterDetails, false, "1");
                string _Id = "";

                #region data update
                if (dsProductItemParameterDetails.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("ProductItemParameterDetails", out _Id);
                    _Id = "PIP" + _Id;
                    ParameterData["Id"] = _Id;
                    ParameterData["ItemId"] = Pid;
                    AddNewRow(dsProductItemParameterDetails.Tables[0], ParameterData);
                }
                else
                {
                    _Id = ParameterData["Id"].ToString();
                    ParameterData["ItemId"] = Pid;
                    EditRow(dsProductItemParameterDetails.Tables[0].Rows[0], ParameterData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsProductItemParameterDetails);

                return Json(new { Error = false, Data = ParameterData, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        #endregion -- Operations
    }
}