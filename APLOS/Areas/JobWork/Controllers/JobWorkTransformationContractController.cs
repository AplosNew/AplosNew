using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkTransformationContractController : BaseController
    {
        
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkTransformationContractController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: JobWork/JobWorkTransformationContract
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Dropdown Code Area
        [HttpGet, Authorize]
        public JsonResult LoadProcessType()
        {
            string sql = "";
            sql = @"SELECT Id,StandardName FROM HKP.ProcessType ORDER BY StandardName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetByProductApplicable()
        {
            string sql = "";
            sql = @"SELECT DISTINCT ByProductApplicable FROM [MST].[JobWorkTransformationMaster] ORDER BY ByProductApplicable";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobWorkMaterialList()
        {
            string sql = "";
            sql = @"SELECT M.Id, M.UserName MaterialName FROM HKP.JobWorkItem I
                    INNER JOIN MST.MaterialMaster M ON M.Id = I.MaterialMasterId
                    ORDER BY M.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobWorkMaterialListByProduct()
        {
            string sql = "";
            sql = @"SELECT M.Id, M.UserName MaterialName FROM HKP.JobWorkItem I
                    INNER JOIN MST.MaterialMaster M ON M.Id = I.MaterialMasterId
                    ORDER BY M.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetJobWorkMaterialUOM(string Id)
        {
            string sql = "";
            sql = @"SELECT UserName FROM SCS.UnitOfMeasurement WHERE Id =(SELECT UOMId FROM HKP.JobWorkItem WHERE MaterialMasterId='" + Id + "')";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult LoadAllPlant()
        {
            string sql = "";
            sql = @"SELECT Id,StandardName FROM ORG.Plant ORDER BY StandardName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult LoadPlantWiseEntity(string Id)
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM ORG.Entity WHERE PlantId='" + Id + "' ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LoadMaterialLocation(string PlantId, string EntityId)
        {
            string sql = "";
            sql = @"SELECT Id,LocationName FROM [HKP].[JobWorkLocation] WHERE PlantId='" + PlantId + "' AND EntityId='" + EntityId + "' ORDER BY LocationName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Save Area
        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData, List<Dictionary<string, string>> materialPlanning, List<Dictionary<string, string>> materialInput)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {

                if (materialPlanning == null || materialPlanning.Count == 0)
                    throw new Exception("No child data found");

                DataTable dtMaterialPlanning = new DataTable();

                var columnNames = materialPlanning.SelectMany(dict => dict.Keys).Distinct();
                dtMaterialPlanning.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
                foreach (Dictionary<string, string> item in materialPlanning)
                {
                    var row = dtMaterialPlanning.NewRow();
                    foreach (var key in item.Keys)
                    {
                        row[key] = item[key];
                    }

                    dtMaterialPlanning.Rows.Add(row);
                }

                if (materialInput == null || materialInput.Count == 0)
                    throw new Exception("No Material Input data found");

                DataTable dtmaterialInput = new DataTable();

                var materialInputColumnNames = materialInput.SelectMany(dict => dict.Keys).Distinct();
                dtmaterialInput.Columns.AddRange(materialInputColumnNames.Select(c => new DataColumn(c)).ToArray());
                foreach (Dictionary<string, string> item in materialInput)
                {
                    var row = dtmaterialInput.NewRow();
                    foreach (var key in item.Keys)
                    {
                        row[key] = item[key];
                    }

                    dtmaterialInput.Rows.Add(row);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM [MST].[JobWorkTransformationContract] WHERE Id='" + saveData["Id"].ToString() + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[MST].[JobWorkTransformationContract]", out Id);
                    Id = "JWTC-" + Id;
                    dr["Id"] = Id.ToString();
                    dr["ProcessTypeId"] = saveData["ProcessTypeId"].ToString();
                    dr["ContractDate"] = saveData["ContractDate"].ToString();
                    dr["ContractTime"] = saveData["ContractTime"].ToString();
                    dr["PlantId"] = saveData["PlantId"].ToString();
                    dr["EntityId"] = saveData["EntityId"].ToString();
                    dr["JobWorkLocationId"] = saveData["JobWorkLocationId"].ToString();
                    dr["MaterialType"] = saveData["MaterialType"].ToString();
                    dr["FinalOutputCategory"] = saveData["FinalOutputCategory"].ToString();
                    dr["PartyId"] = saveData["PartyId"].ToString();
                    dr["ProcessStartDate"] = saveData["ProcessStartDate"].ToString();
                    dr["ProcessEndDate"] = saveData["ProcessEndDate"].ToString();
                    dr["ContractClosingDate"] = saveData["ContractClosingDate"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].Rows[0];
                    Id = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["ProcessTypeId"] = saveData["ProcessTypeId"].ToString();
                    dr["ContractDate"] = saveData["ContractDate"].ToString();
                    dr["ContractTime"] = saveData["ContractTime"].ToString();
                    dr["PlantId"] = saveData["PlantId"].ToString();
                    dr["EntityId"] = saveData["EntityId"].ToString();
                    dr["JobWorkLocationId"] = saveData["JobWorkLocationId"].ToString();
                    dr["MaterialType"] = saveData["MaterialType"].ToString();
                    dr["FinalOutputCategory"] = saveData["FinalOutputCategory"].ToString();
                    dr["PartyId"] = saveData["PartyId"].ToString();
                    dr["ProcessStartDate"] = saveData["ProcessStartDate"].ToString();
                    dr["ProcessEndDate"] = saveData["ProcessEndDate"].ToString();
                    dr["ContractClosingDate"] = saveData["ContractClosingDate"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                con.getDataSet("Select * from [MST].[JobWorkTransformationContractMaterialPlanning] where JobWorkTransformationContractId ='" + Id.ToString() + "'", out DataSet dsMaterialPlanning);
                if (dtMaterialPlanning == null || dtMaterialPlanning.Rows.Count == 0)
                {
                    while (dsMaterialPlanning.Tables[0].DefaultView.Count > 0)
                    {
                        dsMaterialPlanning.Tables[0].DefaultView[0].Delete();
                    }
                }

                for (int i = 0; i < dsMaterialPlanning.Tables[0].Rows.Count; i++)
                {
                    dtMaterialPlanning.DefaultView.RowFilter = "Id='" + dsMaterialPlanning.Tables[0].Rows[i]["Id"].ToString() + "'";
                    if (dtMaterialPlanning.DefaultView.Count == 0)
                        dsMaterialPlanning.Tables[0].Rows[i].Delete();
                }

                string MaterialPlanningId = "";
                for (int i = 0; i < dtMaterialPlanning.Rows.Count; i++)
                {

                    dsMaterialPlanning.Tables[0].DefaultView.RowFilter = "Id='" + dtMaterialPlanning.Rows[i]["Id"].ToString() + "'";
                    if (dsMaterialPlanning.Tables[0].DefaultView.Count == 0)
                    {
                        //addnew
                        DataRow dr = dsMaterialPlanning.Tables[0].NewRow();

                        _genId.GenID("[MST].[JobWorkTransformationContractMaterialPlanning]", out MaterialPlanningId);
                        MaterialPlanningId = "JWTMP-" + MaterialPlanningId;

                        
                        dr["Id"] = MaterialPlanningId;

                        dr["JobWorkTransformationContractId"] = Id;
                        dr["JobWorkMaterialMasterId"] = dtMaterialPlanning.Rows[i]["JobWorkMaterialMasterId"].ToString();
                        dr["MaterialSpecification"] = dtMaterialPlanning.Rows[i]["MaterialSpecification"].ToString();
                        dr["MaterialRef"] = dtMaterialPlanning.Rows[i]["MaterialRef"].ToString();
                        dr["UOM"] = dtMaterialPlanning.Rows[i]["UOM"].ToString();
                        dr["Quantity"] = dtMaterialPlanning.Rows[i]["Quantity"].ToString();
                        dr["ArticleCode"] = dtMaterialPlanning.Rows[i]["MaterialMasterId"].ToString();
                        dr["OrderSpecific"] = dtMaterialPlanning.Rows[i]["OrderSpecific"].ToString();
                        dr["RequiredCapacityPerDay"] = dtMaterialPlanning.Rows[i]["RequiredCapacityPerDay"].ToString();
                        dr["ByProductApplicable"] = dtMaterialPlanning.Rows[i]["ByProductApplicable"].ToString();
                        dr["RateApply"] = dtMaterialPlanning.Rows[i]["RateApply"].ToString();
                        dr["CurrencyId"] = dtMaterialPlanning.Rows[i]["CurrencyId"].ToString();
                        dr["RatePerUnit"] = dtMaterialPlanning.Rows[i]["RatePerUnit"].ToString();
                        dr["Rejection"] = dtMaterialPlanning.Rows[i]["Rejection"].ToString();
                        dr["ValueLoss"] = dtMaterialPlanning.Rows[i]["ValueLoss"].ToString();
                        dr["ResponsiblePersonId"] = dtMaterialPlanning.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtMaterialPlanning.Rows[i]["Remarks"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaterialPlanning.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaterialPlanning.Tables[0].Rows[i];
                        dr.BeginEdit();

                        dr["JobWorkTransformationContractId"] = saveData["Id"].ToString();
                        dr["JobWorkMaterialMasterId"] = dtMaterialPlanning.Rows[i]["JobWorkMaterialMasterId"].ToString();
                        dr["MaterialSpecification"] = dtMaterialPlanning.Rows[i]["MaterialSpecification"].ToString();
                        dr["MaterialRef"] = dtMaterialPlanning.Rows[i]["MaterialRef"].ToString();
                        dr["UOM"] = dtMaterialPlanning.Rows[i]["UOM"].ToString();
                        dr["Quantity"] = dtMaterialPlanning.Rows[i]["Quantity"].ToString();
                        dr["ArticleCode"] = dtMaterialPlanning.Rows[i]["MaterialMasterId"].ToString();
                        dr["OrderSpecific"] = dtMaterialPlanning.Rows[i]["OrderSpecific"].ToString();
                        dr["RequiredCapacityPerDay"] = dtMaterialPlanning.Rows[i]["RequiredCapacityPerDay"].ToString();
                        dr["ByProductApplicable"] = dtMaterialPlanning.Rows[i]["ByProductApplicable"].ToString();
                        dr["RateApply"] = dtMaterialPlanning.Rows[i]["RateApply"].ToString();
                        dr["CurrencyId"] = dtMaterialPlanning.Rows[i]["CurrencyId"].ToString();
                        dr["RatePerUnit"] = dtMaterialPlanning.Rows[i]["RatePerUnit"].ToString();
                        dr["Rejection"] = dtMaterialPlanning.Rows[i]["Rejection"].ToString();
                        dr["ValueLoss"] = dtMaterialPlanning.Rows[i]["ValueLoss"].ToString();
                        dr["ResponsiblePersonId"] = dtMaterialPlanning.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtMaterialPlanning.Rows[i]["Remarks"].ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

                con.getDataSet("Select * from [MST].[JobWorkTransformationContractMaterialInput] where JobWorkTransformationContractId ='" + Id.ToString() + "'", out DataSet dsMaterialInput);
                if (dtmaterialInput == null || dtmaterialInput.Rows.Count == 0)
                {
                    while (dsMaterialInput.Tables[0].DefaultView.Count > 0)
                    {
                        dsMaterialInput.Tables[0].DefaultView[0].Delete();
                    }
                }

                for (int i = 0; i < dsMaterialInput.Tables[0].Rows.Count; i++)
                {
                    dtmaterialInput.DefaultView.RowFilter = "Id='" + dsMaterialInput.Tables[0].Rows[i]["Id"].ToString() + "'";
                    if (dtmaterialInput.DefaultView.Count == 0)
                        dsMaterialInput.Tables[0].Rows[i].Delete();
                }

                string MaterialInputId = "";
                for (int i = 0; i < dtmaterialInput.Rows.Count; i++)
                {

                    dsMaterialInput.Tables[0].DefaultView.RowFilter = "Id='" + dtmaterialInput.Rows[i]["Id"].ToString() + "'";
                    if (dsMaterialInput.Tables[0].DefaultView.Count == 0)
                    {
                        //addnew
                        DataRow dr = dsMaterialInput.Tables[0].NewRow();

                        _genId.GenID("[MST].[JobWorkTransformationContractMaterialInput]", out MaterialInputId);
                        MaterialInputId = "JWTMP-" + MaterialInputId;


                        dr["Id"] = MaterialInputId;

                        dr["JobWorkTransformationContractId"] = Id;
                        dr["JobWorkMaterialId"] = dtmaterialInput.Rows[i]["JobWorkMaterialId"].ToString();
                        dr["MaterialSpecification"] = dtmaterialInput.Rows[i]["MaterialSpecification"].ToString();
                        dr["UOM"] = dtmaterialInput.Rows[i]["UOM"].ToString();
                        dr["NetConsumption"] = dtmaterialInput.Rows[i]["NetConsumption"].ToString();
                        dr["Rejection"] = dtmaterialInput.Rows[i]["Rejection"].ToString();
                        dr["ValueLoss"] = dtmaterialInput.Rows[i]["ValueLoss"].ToString();
                        dr["GrossConsumption"] = dtmaterialInput.Rows[i]["GrossConsumption"].ToString();
                        dr["ResponsiblePersonId"] = dtmaterialInput.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtmaterialInput.Rows[i]["Remarks"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaterialInput.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaterialInput.Tables[0].Rows[i];
                        dr.BeginEdit();

                        dr["JobWorkTransformationContractId"] = saveData["Id"].ToString();
                        dr["JobWorkMaterialId"] = dtmaterialInput.Rows[i]["JobWorkMaterialId"].ToString();
                        dr["MaterialSpecification"] = dtmaterialInput.Rows[i]["MaterialSpecification"].ToString();
                        dr["UOM"] = dtmaterialInput.Rows[i]["UOM"].ToString();
                        dr["NetConsumption"] = dtmaterialInput.Rows[i]["NetConsumption"].ToString();
                        dr["Rejection"] = dtmaterialInput.Rows[i]["Rejection"].ToString();
                        dr["ValueLoss"] = dtmaterialInput.Rows[i]["ValueLoss"].ToString();
                        dr["GrossConsumption"] = dtmaterialInput.Rows[i]["GrossConsumption"].ToString();
                        dr["ResponsiblePersonId"] = dtmaterialInput.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtmaterialInput.Rows[i]["Remarks"].ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut, dsMaterialPlanning, dsMaterialInput);

                return Json(new { Error = false, Message = _Message, Id = Id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Load Data
        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"SELECT C.Id,P.StandardName ProcessTypeId,ContractDate,ContractTime,P.UserName PlantId,E.UserName EntityId,
                    L.LocationName JobWorkLocationId,MaterialType,FinalOutputCategory,PR.ShortName PartyId,ProcessStartDate,
                    ProcessEndDate,ContractClosingDate,C.Remarks
                    FROM [MST].[JobWorkTransformationContract] C
                    LEFT JOIN HKP.ProcessType P ON P.Id = C.ProcessTypeId
                    LEFT JOIN ORG.Plant PL ON PL.Id = C.PlantId
                    LEFT JOIN ORG.Entity E ON E.Id = C.EntityId
                    LEFT JOIN [HKP].JobWorkLocation L ON L.Id = C.JobWorkLocationId
                    LEFT JOIN [HKP].[Party] PR ON PR.Id = C.PartyId
                    ORDER BY P.StandardName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"Select C.Id,ProcessTypeId,ContractDate,ContractTime,PlantId,EntityId,JobWorkLocationId,MaterialType,FinalOutputCategory,
                    PartyId,PR.UserName PartyName,PR.Code PartyCode,ProcessStartDate,ProcessEndDate,ContractClosingDate,C.Remarks 
                    FROM [MST].[JobWorkTransformationContract] C
                    LEFT JOIN [HKP].[Party] PR ON PR.Id = C.PartyId
                    WHERE C.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRateApplicable()
        {
            string sql = "";
            sql = @"SELECT DISTINCT RateApplicable FROM [MST].[JobWorkValueAddedMaster] ORDER BY RateApplicable";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetValueAddedMasterItem()
        {
            string sql = "";
            sql = @"SELECT I.Id, M.UserName ItemName
                    FROM [MST].JobWorkMaterialInput I
                    INNER JOIN MST.MaterialMaster M ON M.Id = I.JobWorkMaterialId
                    ORDER BY M.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetValueAddedCurrency()
        {
            string sql = "";
            sql = @"SELECT Id,Code FROM SCS.Currency
                    WHERE Id IN (SELECT DISTINCT CurrencyId FROM [MST].[JobWorkValueAddedMaster])";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBuyerList()
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM [HKP].[Buyer] WHERE Active=1 ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedByProduct(string Id)
        {
            string sql = "";
            sql = @"SELECT P.Id,JobWorkTransformationContractMaterialPlanningId,JobWorkMaterialId,M.UserName MaterialName,MaterialSpecification,UOM,StandardQty,Rejection,
                    ValueLoss,GrossQty,CurrencyId,C.Code Currency,StandardRate,ResponsiblePersonId,E.EmployeeName ResponsiblePerson,P.Remarks
                    FROM MST.JobWorkTransformationContractByProduct P
                    INNER JOIN dbo.EmployeeInformation E ON E.SystemId = P.ResponsiblePersonId
                    INNER JOIN MST.MaterialMaster M ON M.Id = P.JobWorkMaterialId
                    INNER JOIN SCS.Currency C ON C.Id = P.CurrencyId
                    WHERE P.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedValueLossRejectionUOM(string Id)
        {
            string sql = "";
            sql = @"SELECT I.Rejection StdRejection,I.ValueLoss StdValueLoss,U.UserName UOMName,M.RateApplicable,M.CurrencyId
                    FROM [MST].[JobWorkTransformationMaster] M
                    INNER JOIN MST.JobWorkMaterialInput I ON I.JobWorkTransformationMasterId = M.Id
                    INNER JOIN [SCS].[UnitOfMeasurement] U ON U.Id = M.MaterialUOMId
                    WHERE I.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetMaterialPlanningData(string Id)
        {
            string sql = "";
            sql = @"SELECT P.Id,P.JobWorkTransformationContractId,P.JobWorkMaterialMasterId,MM.UserName JobWorkMaterialName,P.MaterialSpecification,
                    P.MaterialRef,P.UOM,P.Quantity,P.ArticleCode MaterialMasterId,MA.Code MaterialMasterName,P.OrderSpecific,P.ByProductApplicable,
                    P.RequiredCapacityPerDay,P.RateApply,P.CurrencyId,C.Code Currency,P.RatePerUnit,
                    P.Rejection,P.ValueLoss,P.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,P.Remarks
                    FROM MST.JobWorkTransformationContractMaterialPlanning P
                    INNER JOIN MST.JobWorkMaterialInput MI ON MI.Id = P.JobWorkMaterialMasterId
                    INNER JOIN MST.JobWorkTransformationMaster M ON M.Id = MI.JobWorkTransformationMasterId
                    INNER JOIN HKP.JobWorkActivity A ON A.Id = M.JobWorkActivityId
                    INNER JOIN MST.MaterialMaster MM ON MM.Id  = P.ArticleCode 
                    INNER JOIN MST.MaterialMasterArticle MA ON MA.MaterialMasterId = MM.Id
                    INNER JOIN SCS.Currency C ON C.Id = P.CurrencyId
                    INNER JOIN EmployeeInformation E ON E.SystemId = P.ResponsiblePersonId
                    WHERE P.JobWorkTransformationContractId='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetMaterialInputData(string Id)
        {
            string sql = "";
            sql = @"SELECT I.Id,I.JobWorkTransformationContractId,I.JobWorkMaterialId,M.UserName JobWorkMaterialName,I.MaterialSpecification,
                    I.UOM,I.NetConsumption,I.Rejection,I.ValueLoss,I.GrossConsumption,I.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,
                    I.Remarks
                    FROM [MST].[JobWorkTransformationContractMaterialInput] I
                    INNER JOIN MST.MaterialMaster M ON M.Id = I.JobWorkMaterialId
                    INNER JOIN dbo.EmployeeInformation E ON E.SystemId = I.ResponsiblePersonId
                    WHERE JobWorkTransformationContractId='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedMaterialInputData(string Id)
        {
            string sql = "";
            sql = @"SELECT I.Id,I.JobWorkTransformationContractId,I.JobWorkMaterialId,M.UserName JobWorkMaterialName,I.MaterialSpecification,
                    I.UOM,I.NetConsumption,I.Rejection,I.ValueLoss,I.GrossConsumption,I.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,
                    I.Remarks
                    FROM [MST].[JobWorkTransformationContractMaterialInput] I
                    INNER JOIN MST.MaterialMaster M ON M.Id = I.JobWorkMaterialId
                    INNER JOIN dbo.EmployeeInformation E ON E.SystemId = I.ResponsiblePersonId
                    WHERE I.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetMaterialPlanningAttachment(string Id)
        {
            string sql = "";
            sql = @"SELECT A.Id,A.JobWorkTransformationContractMaterialPlanningId,A.OrginalFileName,A.FileName 
                    FROM [MST].[JobWorkTransformationContractMaterialPlanningAttachment] A
                    INNER JOIN [MST].[JobWorkTransformationContractMaterialPlanning] M ON M.Id = A.JobWorkTransformationContractMaterialPlanningId
                    WHERE A.JobWorkTransformationContractMaterialPlanningId='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult SaveMaterialPlanningAttachment(HttpPostedFileBase[] file, string Id)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var _OriginalFileName = "";
                var _NewFileName = "";
                string _AttachmentId = "";

                var files = System.Web.HttpContext.Current.Request.Files["file"];

                if (!string.IsNullOrEmpty(Id.ToString()))
                {
                    if (files != null)
                    {
                        if (files.ContentLength > 0)
                        {
                            _OriginalFileName = files.FileName;

                            string extension = Path.GetExtension(files.FileName);

                            _NewFileName = Id.Replace("-", "_").Replace(".", "_") + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetExtension(files.FileName);
                            var path = Path.Combine(ResourcesPathReader.GetVASPath(), _NewFileName);
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                                files.SaveAs(path);
                            }
                            else
                            {
                                files.SaveAs(path);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("File Not Found..!");
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();

                con.getDataSet("Select * from [MST].[JobWorkTransformationContractMaterialPlanningAttachment] where Id=''", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[MST].[JobWorkTransformationContractMaterialPlanningAttachment]", out _AttachmentId);
                    _AttachmentId = "MPA-" + _AttachmentId;
                    dr["Id"] = _AttachmentId;
                    dr["JobWorkTransformationContractMaterialPlanningId"] = Id;
                    dr["OrginalFileName"] = _OriginalFileName.ToString();
                    dr["FileName"] = _NewFileName.ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                }
                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);
                return Json(new { Error = false, Message = "Data Save successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteMaterialPlanningAttachment(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkTransformationContractMaterialPlanningAttachment] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkValueAddedContract] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteMaterialPlanningChildData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkMaterialPlanning] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteMaterialInputChildData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkTransformationContractMaterialInput] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult SaveMaterialPlanningRequirement(Dictionary<string, object> saveData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM [MST].[JobWorkTransformationContractRequirements] WHERE Id='" + saveData["Id"].ToString() + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[MST].[JobWorkTransformationContractRequirements]", out Id);
                    Id = "JWVAC-" + Id;
                    dr["Id"] = Id.ToString();
                    dr["JobWorkTransformationContractMaterialPlanningId"] = saveData["JobWorkTransformationContractMaterialPlanningId"].ToString();
                    dr["OrderType"] = saveData["OrderType"].ToString();
                    dr["CustomerId"] = saveData["CustomerId"].ToString();
                    dr["ProductionOrderId"] = saveData["ProductionOrderId"].ToString();
                    dr["Specification"] = saveData["Specification"].ToString();
                    dr["OutputMaterialUOM"] = saveData["OutputMaterialUOM"].ToString();
                    dr["Quantity"] = saveData["Quantity"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].Rows[0];
                    Id = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["JobWorkTransformationContractMaterialPlanningId"] = saveData["JobWorkTransformationContractMaterialPlanningId"].ToString();
                    dr["OrderType"] = saveData["OrderType"].ToString();
                    dr["CustomerId"] = saveData["CustomerId"].ToString();
                    dr["ProductionOrderId"] = saveData["ProductionOrderId"].ToString();
                    dr["Specification"] = saveData["Specification"].ToString();
                    dr["OutputMaterialUOM"] = saveData["OutputMaterialUOM"].ToString();
                    dr["Quantity"] = saveData["Quantity"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);

                return Json(new { Error = false, Message = _Message, Id = Id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveMaterialPlanningByProduct(Dictionary<string, object> saveData)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                string Id = "";
                con.getDataSet("SELECT * FROM [MST].[JobWorkTransformationContractByProduct] WHERE Id='" + saveData["Id"].ToString() + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[MST].[JobWorkTransformationContractByProduct]", out Id);
                    Id = "TCBP-" + Id;
                    dr["Id"] = Id.ToString();
                    dr["JobWorkTransformationContractMaterialPlanningId"] = saveData["JobWorkTransformationContractMaterialPlanningId"].ToString();
                    dr["JobWorkMaterialId"] = saveData["JobWorkMaterialId"].ToString();
                    dr["MaterialSpecification"] = saveData["MaterialSpecification"].ToString();
                    dr["UOM"] = saveData["UOM"].ToString();
                    dr["StandardQty"] = saveData["StandardQty"].ToString();
                    dr["Rejection"] = saveData["Rejection"].ToString();
                    dr["ValueLoss"] = saveData["ValueLoss"].ToString();
                    dr["GrossQty"] = saveData["GrossQty"].ToString();
                    dr["CurrencyId"] = saveData["CurrencyId"].ToString();
                    dr["StandardRate"] = saveData["StandardRate"].ToString();
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].Rows[0];
                    Id = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["JobWorkTransformationContractMaterialPlanningId"] = saveData["JobWorkTransformationContractMaterialPlanningId"].ToString();
                    dr["JobWorkMaterialId"] = saveData["JobWorkMaterialId"].ToString();
                    dr["MaterialSpecification"] = saveData["MaterialSpecification"].ToString();
                    dr["UOM"] = saveData["UOM"].ToString();
                    dr["StandardQty"] = saveData["StandardQty"].ToString();
                    dr["Rejection"] = saveData["Rejection"].ToString();
                    dr["ValueLoss"] = saveData["ValueLoss"].ToString();
                    dr["GrossQty"] = saveData["GrossQty"].ToString();
                    dr["CurrencyId"] = saveData["CurrencyId"].ToString();
                    dr["StandardRate"] = saveData["StandardRate"].ToString();
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut);

                return Json(new { Error = false, Message = _Message}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteSelectedByProductRow(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkTransformationContractByProduct] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetAllMaterialPlanningRequirements(string Id)
        {
            string sql = "";
            sql = @"SELECT R.Id,R.JobWorkTransformationContractMaterialPlanningId, R.OrderType,B.UserName Buyer,R.ProductionOrderId,PO.MasterOrderId,R.Specification,
                    R.OutputMaterialUOM,R.Quantity,R.Remarks
                    FROM [MST].[JobWorkTransformationContractRequirements] R
                    INNER JOIN HKP.Buyer B ON B.Id = R.CustomerId
                    INNER JOIN(
	                    SELECT DISTINCT PO.Id,SO.MasterOrderId FROM [TRN].[ProductionOrder] AS PO
	                    LEFT OUTER  JOIN (
	                    SELECT pod.ProductionOrderId,
	                    MasterOrderId=STUFF((SELECT DISTINCT ','+XMOI.MasterOrderId from 
						                    trn.MasterOrderItem XMOI 	 
						                    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
						                    INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
						                    WHERE podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')											                   
						                    FROM trn.SalesOrder SO 
	                    JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
	                    ) AS SO ON so.ProductionOrderId=po.Id
	                    LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                    )PO ON PO.Id = R.ProductionOrderId
                    WHERE R.JobWorkTransformationContractMaterialPlanningId='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedMaterialPlanningRequirements(string Id)
        {
            string sql = "";
            sql = @"SELECT R.Id,R.JobWorkTransformationContractMaterialPlanningId, R.OrderType,R.CustomerId,B.UserName Buyer,R.ProductionOrderId,PO.MasterOrderId,R.Specification,
                    R.OutputMaterialUOM,R.Quantity,R.Remarks
                    FROM [MST].[JobWorkTransformationContractRequirements] R
                    INNER JOIN HKP.Buyer B ON B.Id = R.CustomerId
                    INNER JOIN(
	                    SELECT DISTINCT PO.Id,SO.MasterOrderId FROM [TRN].[ProductionOrder] AS PO
	                    LEFT OUTER JOIN (
	                    SELECT pod.ProductionOrderId,
	                    MasterOrderId=STUFF((SELECT DISTINCT ','+XMOI.MasterOrderId from 
						                    trn.MasterOrderItem XMOI
						                    INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
						                    INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
						                    WHERE podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')											                   
						                    FROM trn.SalesOrder SO 
	                    JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
	                    ) AS SO ON so.ProductionOrderId=po.Id
	                    LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                    )PO ON PO.Id = R.ProductionOrderId
                    WHERE R.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetByProductList(string Id)
        {
            string sql = "";
            sql = @"SELECT I.Id,JobWorkTransformationContractMaterialPlanningId,JobWorkMaterialId,M.UserName MaterialName,MaterialSpecification,
                    UOM,StandardQty,Rejection,ValueLoss,GrossQty,CurrencyId,C.Code Currency,StandardRate,ResponsiblePersonId,E.EmployeeName ResponsiblePerson,I.Remarks
                    FROM MST.JobWorkTransformationContractByProduct I
                    INNER JOIN MST.MaterialMaster M ON M.Id = JobWorkMaterialId
                    INNER JOIN SCS.Currency C ON C.Id = I.CurrencyId
                    JOIN dbo.EmployeeInformation E ON E.SystemId = I.ResponsiblePersonId
                    WHERE I.JobWorkTransformationContractMaterialPlanningId='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult DeleteMaterialPlanningRequirements(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkValueAddedContractRequirements] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionOrder()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            sql = @"SELECT 
                    case when PO.PlantId='" + identity.PlantId + @"' AND PO.PlantId=EN.PlantId then 'OWN' else 
                    case when PO.PlantId='" + identity.PlantId + @"' and EN.PlantId<>PO.PlantId then 'OUT' ELSE
                    case when PO.PlantId<>'" + identity.PlantId + @"' AND EN.PlantId='" + identity.PlantId + @"' THEN 'IN' ELSE '' END END END AS Owner,
                    PO.*,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName, 

                                        isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                                                    -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                                                    sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     
from 
 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                                    group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}