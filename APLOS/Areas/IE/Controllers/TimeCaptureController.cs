using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System.Data;
using Newtonsoft.Json;
using OTSBD;

namespace Aplos.Areas.IE.Controllers
{
    public class TimeCaptureController : Controller
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;
        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;

        public TimeCaptureController(
            IOperationTimeCaptureMasterService operationTimeCaptureService
            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
            , IOperationService operationService
            , IOperationVariationService operationStepService
            , ISqlRepository sqlRepository)
        {
            _operationStepService = operationStepService;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _ioperationtimecaptureservice = operationTimeCaptureService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetOperationCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVasVersion(string operationId)
        {
            return Json(_operationtimecapturedetailservice.GetVasVersion(operationId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAllVersion(string operationId)
        {
            return Json(_operationtimecapturedetailservice.GetAllVersion(operationId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMasterData(string masterid)
        {
            return Json(_ioperationtimecaptureservice.GetMasterData(masterid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetTimeCapturedetail(string masterid)
        {
            return Json(_ioperationtimecaptureservice.GetDetailList(masterid), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_ioperationtimecaptureservice.GetSearchData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form)
        {
            try
            {
                var file = Request.Files["file"];
                var m = new JavaScriptSerializer().Deserialize<OperationTimeCaptureMaster>(form["model"]);
                IList<OperationTimeCaptureDetail> detailList = System.Web.Helpers.Json.Decode<List<OperationTimeCaptureDetail>>(form["detail"]);
                var extension = Path.GetExtension(file.FileName);
                if (file == null)
                    throw (new Exception("File is null"));
                if (extension.ToUpper() != ".WEBM")
                    throw (new Exception("Please upload .webm file only"));
                var _fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetExtension(file.FileName);
                m.OperationVideoUploadId = _fileName;
                m.FileExtension = extension;
                _ioperationtimecaptureservice.Insert(m, detailList);

                //To Do change path
                var path = Path.Combine(ResourcesPathReader.GetMaterialsImagePath(), _fileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
                else
                    file.SaveAs(path);

                return Json(new { OperationTimeCaptureMaster = m, Message = AplosMessage.Insert });
            }
            catch (CustomException)
            {
                throw;
            }
        }

        [HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        public JsonResult CreateMasterDetail(OperationTimeCaptureMaster m, IList<OperationTimeCaptureDetail> cl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    throw new CustomException(Resources.RequiredFieldMessage);
                }

                _ioperationtimecaptureservice.Insert(m, cl);
                return Json(new { operationtimecapturemaster = m, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Edit(OperationTimeCaptureMaster operationtimecapturemaster)
        {
            _ioperationtimecaptureservice.Update(operationtimecapturemaster);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _ioperationtimecaptureservice.Archive(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
            //if (!string.IsNullOrEmpty(id))
            //{
            //    _ioperationtimecaptureservice.Archive(id);
            //    return Json(new { });
            //}
            //else
            //    throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations

        #region Forhad Code
        [HttpGet, Authorize]
        public JsonResult GetOperationList()
        {
            string sql = "";
            sql = @"SELECT ov.ArticleId,m.MaterialMasterId,mm.UserName AS MachineMaster,M.StandardName MachineName,OV.Id, OV.Code,VM.OperationVariationSystemId,OV.StandardName OperationVariationName, ISNULL(OV.Frequency,0)Frequency,ISNULL(OV.SPI,0)SPI,O.UserName OperationName,ps.UserName AS ProductionSystem,
                    ISNULL(O.PersonalAllowance,0)PersonalAllowance,ISNULL(OV.MachineAllowance,0)MachineAllowance,ISNULL(OV.AdditionalAllowance,0) AdditionalAllowances,
                    ISNULL(M.RPM,0)RPM,OV.TotalSAM,TotalVersion,VM.[Version] AS ApprovedVersion,O.IsMachineRequired,
                    UPPER(VM.ApprovedBy)ApprovedBy,convert(varchar, VM.ApprovedDate, 0)ApprovedDate,
                    UPPER(VM.AddedBy)AddedBy,convert(varchar, VM.AddedDate, 0)AddedDate
                    FROM [MST].[OperationVariation] OV
                    left join [MST].[VASMaster] VM on VM.OperationVariationSystemId=OV.Id and VM.Id=(select Top 1 Id from [MST].[VASMaster] XM where XM.OperationVariationSystemId=OV.Id AND isnull(IsApproved,0)=1)
                    LEFT JOIN [MST].[MaterialMasterArticle] M ON M.Id = OV.ArticleId
                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=m.MaterialMasterId
                    LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                    LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=o.ProductionSystemId
                    LEFT JOIN (SELECT OperationVariationSystemId,Count(Version)TotalVersion FROM [MST].VASMaster GROUP BY OperationVariationSystemId) OP ON OP.OperationVariationSystemId = OV.Id ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedOperation(string OperationVariationSystemId, string Version)
        {
            string sql = "";
            sql = @"SELECT MMA.Id AS ArticleId,MMA.MaterialMasterId,mm.UserName AS MachineMaster,MMA.StandardName MachineName, M.Id,M.IsApproved,OV.Code AS OperationCode,M.OperationVariationSystemId, ov.TotalSAM AS OperationSAM,VASSAM,StandardSAM,M.ProductionSystemId,M.FactorValue BHTValue,AvgMaxMin,
                    ISNULL(M.Frequency,0)Frequency,ISNULL(M.SPI,0)SPI,ISNULL(M.RPM,0)RPM,ISNULL(MachineAllowances,0)MachineAllowances,M.VasDescription,
                    ISNULL(PersonalAllowances,0)PersonalAllowances,ISNULL(AdditionalAllowances,0)AdditionalAllowances,IsAvgCT1,IsAvgCT2,IsAvgCT3,IsAvgCT4,IsAvgCT5,M.Version,
                    ElementID,E.ShortName AS ElementType,UPPER(M.ApprovedBy)ApprovedBy,convert(varchar, M.ApprovedDate, 0)ApprovedDate,
                    UPPER(M.AddedBy)AddedBy,convert(varchar, M.AddedDate, 0)AddedDate,isnull(M.VASQuantity,1) AS VASQuantity,
                    ECODE.Code AS ElementCode,C.TMU,CT1,CT2,CT3,CT4,CT5,TimeAvg,Ratings,BasicTime,M.OperatorId,M.Remarks,M.VASVideoName,M.OriginalVideoName
                    FROM [MST].[VASMaster] M
                    INNER JOIN [MST].[VASChild] C ON C.VASMasterID = M.Id AND C.Version=M.Version
                    inner join [MST].[OperationVariation] OV on M.OperationVariationSystemId=OV.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = isnull(M.ArticleId,OV.ArticleId)
                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=MMA.MaterialMasterId
               
                    INNER JOIN HKP.ElementType E On E.Id = C.ElementTypeId
                    INNER JOIN HKP.ElementCode ECODE On ECODE.Id = C.ElementID
                    --INNER JOIN [HKP].ProductionSystem B ON B.Id = M.ProductionSystemId
                    WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "' AND M.Version ='" + Version + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetElementType()
        {
            string sql = "";
            sql = @"SELECT Id,UserName name,Description FROM [HKP].[ElementType] ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedOperationVersions(string OperationVariationSystemId)
        {
            string sql = "";
            sql = @"SELECT M.Version,CONCAT(m.Version,CASE WHEN isnull(m.IsApproved,0)=1 THEN '(Approved)' ELSE '' END) AS VersionText,(SELECT ISNULL(MAX(M.Version),0)+1 FROM [MST].[VASMaster] M WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "')MaxVersion " +
                "FROM [MST].[VASMaster] M WHERE Isnull(M.Archive,0)=0 AND M.OperationVariationSystemId='" + OperationVariationSystemId + "' ORDER BY Version";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult ArchiveData(string Id)
        {

            try
            {
                if (string.IsNullOrEmpty(Id) || Id == "null")
                    throw new Exception("Select version first");


                string sql = "select * from mst.VASMaster where Id='" + Id + @"' and isnull(IsApproved,'')=1";
                DataTable dt = _sqlRepository.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                    throw new Exception("System cannot delete approved version");


                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("update mst.VASMaster SET Archive =1 WHERE Id='" + Id + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Data Deleted Successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedProductionSystem(string OperationVariationSystemId)
        {
            string sql = "";
            sql = @"SELECT Id, FactorValue FROM  [HKP].[ProductionSystem]
                    WHERE Id = (SELECT ProductionSystemId 
                    FROM MST.Operation WHERE Id=(SELECT OperationId FROM MST.OperationVariation WHERE Id='" + OperationVariationSystemId + "'))";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult LoadSelectedOperationVersionData(string OperationVariationSystemId)
        {
            string sql = "";
            sql = @"SELECT M.Version,CONCAT(m.Version,CASE WHEN isnull(m.IsApproved,0)=1 THEN '(Approved)' ELSE '' END) AS VersionText FROM [MST].[VASMaster] M WHERE Isnull(M.Archive,0)=0 AND M.OperationVariationSystemId='" + OperationVariationSystemId + "' ORDER BY Version";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetMaxVersion(string OperationVariationSystemId)
        {
            string sql = "";
            sql = @"SELECT ISNULL(MAX(M.Version),0)+1 Version FROM [MST].[VASMaster] M WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "' ORDER BY Version";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetOperationVersionData(string OperationVariationSystemId, int Version)
        {
            string sql = "";
            sql = @"SELECT MMA.Id AS ArticleId,MMA.MaterialMasterId,mm.UserName AS MachineMaster,MMA.StandardName MachineName,C.[Sequence],isnull(M.videoStartTime,'00:00:00') AS videoStartTime, M.Id,OV.Code AS OperationCode,M.OperationVariationSystemId, ov.TotalSAM AS OperationSAM,VASSAM,StandardSAM,M.ProductionSystemId,M.FactorValue BHTValue,
                    AvgMaxMin,M.Frequency,M.SPI,M.RPM,MachineAllowances,M.VasDescription,isnull(M.VASQuantity,1) AS VASQuantity,
                    PersonalAllowances,ISNULL(AdditionalAllowances,0)AdditionalAllowances,IsAvgCT1,IsAvgCT2,IsAvgCT3,IsAvgCT4,
					IsAvgCT5,M.Version,ElementID,E.ShortName AS ElementType,C.ElementTypeId,ECODE.Code AS ElementCode,C.TMU,CT1,CT2,CT3,CT4,CT5,TimeAvg,Ratings,BasicTime,
                    M.OperatorId,M.Remarks,M.VASVideoName,M.OriginalVideoName,M.IsApproved,
                    UPPER(M.AddedBy)AddedBy,convert(varchar, M.AddedDate, 0)AddedDate,
                    UPPER(M.ApprovedBy)ApprovedBy,convert(varchar, M.ApprovedDate, 0)ApprovedDate
                    FROM [MST].[VASMaster] M
                    INNER JOIN [MST].[VASChild] C ON C.VASMasterID = M.Id AND C.Version=M.Version
                    inner join [MST].[OperationVariation] OV on M.OperationVariationSystemId=OV.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = isnull(M.ArticleId,OV.ArticleId)
                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=MMA.MaterialMasterId
                    INNER JOIN HKP.ElementType E On E.Id = C.ElementTypeId
                    INNER JOIN HKP.ElementCode ECODE On ECODE.Id = C.ElementID
                    --INNER JOIN [HKP].ProductionSystem B ON B.Id = M.ProductionSystemId
                    WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "' AND M.Version ='" + Version + "' ORDER BY C.[Sequence]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBHTList()
        {
            string sql = "";
            sql = @"SELECT Id ,UserName Factor FROM [HKP].[ProductionSystem] ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetPSAValue(string ProductionSystemId)
        {
            string sql = "";
            sql = @"SELECT FactorValue FROM [HKP].[ProductionSystem] WHERE Id='" + ProductionSystemId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetGSDCodeList()
        {
            string sql = "";
            sql = @"SELECT Id,Code,ShortName,StandardName,UserName,Description,TMU,MCHand, CodeType  
                    FROM HKP.ElementCode ORDER BY Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        DataTable dtTime = new DataTable();
        [HttpPost, Authorize]
        public ActionResult UploadVideoData(HttpPostedFileBase[] file, string operationData, string operationChild, string CopyVersion, string IsNewVideo)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                string VasMasterId = "";
                var _OriginalVideoName = "";
                var _VASVideoName = "";

                var files = System.Web.HttpContext.Current.Request.Files["file"];
                Dictionary<string, object> operationMaster = JsonConvert.DeserializeObject<Dictionary<string, object>>(operationData);

                if (string.IsNullOrEmpty(operationMaster["Id"].ToString()) && IsNewVideo == "true")
                {
                    if (files != null)
                    {
                        if (files.ContentLength > 0)
                        {
                            _OriginalVideoName = files.FileName;

                            string extension = Path.GetExtension(files.FileName);
                            if (extension.ToUpper() == ".MP4")
                            {
                                _VASVideoName = operationMaster["OperationVariationSystemId"].ToString().Replace("-", "_").Replace(".", "_") + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetExtension(files.FileName);
                                var path = Path.Combine(ResourcesPathReader.GetVASPath(), _VASVideoName);
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                    files.SaveAs(path);
                                }
                                else
                                    files.SaveAs(path);
                            }
                            else
                            {
                                throw new Exception("Please upload .mp4 file only.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Video File Not Found..!");
                    }
                }
                else if (string.IsNullOrEmpty(operationMaster["Id"].ToString()) && !string.IsNullOrEmpty(CopyVersion.ToString()))
                {
                    //con = new ConnectionManager.clsConnection();
                    con.getDataSet(@"SELECT OriginalVideoName,VASVideoName FROM [MST].[VASMaster] WHERE OperationVariationSystemId='" + operationMaster["OperationVariationSystemId"].ToString() + "' AND Version = '" + CopyVersion.ToString() + "'", out DataSet dsVideoName);

                    _OriginalVideoName = dsVideoName.Tables[0].Rows[0]["OriginalVideoName"].ToString();
                    _VASVideoName = dsVideoName.Tables[0].Rows[0]["VASVideoName"].ToString();
                }

                dtTime = CreateEmptyDataTable();

                DataTable dtElementList = _sqlRepository.GetDataTable("select * from hkp.ElementCode");

                string[] vasTimeRows = operationChild.Split('/');
                int Sequence = 0;
                foreach (string vasTimeRow in vasTimeRows)
                {
                    string[] vasTimeRowDetails = vasTimeRow.Split(',');
                    if (vasTimeRowDetails.Length == 12)
                    {
                        if (vasTimeRowDetails[0].ToString() != "" && vasTimeRowDetails[1].ToString() != "")
                        {
                            Sequence++;
                            ConvertModelToDataTable(vasTimeRowDetails, operationMaster, dtElementList, Sequence);
                        }
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //primary key generator
                bplib.clsGenID _genId = new bplib.clsGenID();

                #region VASMaster

                con.getDataSet("Select * from [MST].[VASMaster] where Id='" + operationMaster["Id"].ToString() + "'", out DataSet dsVasMaster);
                if (dsVasMaster.Tables[0].Rows.Count == 0)
                {
                    double Version = 1;
                    con.getDataSet("Select isnull(Max(Version),0)+1 AS Version from [MST].[VASMaster] where OperationVariationSystemId='" + operationMaster["OperationVariationSystemId"].ToString() + "'", out DataSet dsTempVasMaster);
                    if(dsTempVasMaster.Tables[0].Rows.Count>0)
                        Version =clsStaticInfo.dbl( dsTempVasMaster.Tables[0].Rows[0]["Version"]);
                    operationMaster["Version"] = Version;

                    DataRow dr = dsVasMaster.Tables[0].NewRow();
                    _genId.GenID("[MST].[VASMaster]", out VasMasterId);
                    VasMasterId = "VASM" + VasMasterId;
                    dr["Id"] = VasMasterId;
                    operationMaster["Id"] = VasMasterId;

                    dr["OperationVariationSystemId"] = operationMaster["OperationVariationSystemId"].ToString();
                    //dr["OperationSAM"] = OTSBD.clsStaticInfo.dbl(operationMaster["OperationSAM"].ToString());
                    dr["VASSAM"] = OTSBD.clsStaticInfo.dbl(operationMaster["VASSAM"].ToString());
                    dr["videoStartTime"] = operationMaster["videoStartTime"].ToString();
                    dr["StandardSAM"] = OTSBD.clsStaticInfo.dbl(operationMaster["StandardSAM"].ToString());
                    dr["ProductionSystemId"] = operationMaster["ProductionSystemId"].ToString();
                    dr["AvgMaxMin"] = OTSBD.clsStaticInfo.dbl(operationMaster["AvgMaxMin"].ToString());
                    dr["Frequency"] = operationMaster["Frequency"].ToString();
                    dr["SPI"] = operationMaster["SPI"].ToString();
                    dr["RPM"] = operationMaster["RPM"].ToString();
                    dr["OperatorId"] = operationMaster["OperatorId"].ToString();
                    dr["MachineAllowances"] = OTSBD.clsStaticInfo.dbl(operationMaster["MachineAllowances"].ToString());
                    dr["PersonalAllowances"] = OTSBD.clsStaticInfo.dbl(operationMaster["PersonalAllowances"].ToString());
                    dr["AdditionalAllowances"] = OTSBD.clsStaticInfo.dbl(operationMaster["AdditionalAllowances"].ToString());
                    dr["FactorValue"] = OTSBD.clsStaticInfo.dbl(operationMaster["BHTValue"].ToString());
                    dr["VASQuantity"] = OTSBD.clsStaticInfo.dbl(operationMaster["VASQuantity"].ToString());
                    dr["ArticleId"] = operationMaster["ArticleId"];


                    dr["IsAvgCT1"] = operationMaster["IsAvgCT1"].ToString();
                    dr["IsAvgCT2"] = operationMaster["IsAvgCT2"].ToString();
                    dr["IsAvgCT3"] = operationMaster["IsAvgCT3"].ToString();
                    dr["IsAvgCT4"] = operationMaster["IsAvgCT4"].ToString();
                    dr["IsAvgCT5"] = operationMaster["IsAvgCT5"].ToString();
                    dr["Version"] = OTSBD.clsStaticInfo.dbl(operationMaster["Version"].ToString());
                    dr["OriginalVideoName"] = _OriginalVideoName.ToString();
                    dr["VASVideoName"] = _VASVideoName.ToString();
                    dr["Remarks"] = operationMaster["Remarks"] == null ? "" : operationMaster["Remarks"].ToString();
                    dr["VasDescription"] = bplib.clsWebLib.RetValidLen(operationMaster["VasDescription"]);
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["Archive"] = 0;

                    dsVasMaster.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsVasMaster.Tables[0].Rows[0];
                    VasMasterId = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["OperationVariationSystemId"] = operationMaster["OperationVariationSystemId"].ToString();
                    //dr["OperationSAM"] = OTSBD.clsStaticInfo.dbl(operationMaster["OperationSAM"].ToString());
                    dr["VASSAM"] = OTSBD.clsStaticInfo.dbl(operationMaster["VASSAM"].ToString());
                    dr["StandardSAM"] = OTSBD.clsStaticInfo.dbl(operationMaster["StandardSAM"].ToString());
                    dr["ProductionSystemId"] = operationMaster["ProductionSystemId"].ToString();
                    dr["AvgMaxMin"] = OTSBD.clsStaticInfo.dbl(operationMaster["AvgMaxMin"].ToString());
                    dr["Frequency"] = operationMaster["Frequency"].ToString();
                    dr["SPI"] = operationMaster["SPI"].ToString();
                    dr["RPM"] = operationMaster["RPM"].ToString();
                    dr["OperatorId"] = operationMaster["OperatorId"].ToString();
                    dr["MachineAllowances"] = OTSBD.clsStaticInfo.dbl(operationMaster["MachineAllowances"].ToString());
                    dr["PersonalAllowances"] = OTSBD.clsStaticInfo.dbl(operationMaster["PersonalAllowances"].ToString());
                    dr["AdditionalAllowances"] = OTSBD.clsStaticInfo.dbl(operationMaster["AdditionalAllowances"].ToString());
                    dr["FactorValue"] = OTSBD.clsStaticInfo.dbl(operationMaster["BHTValue"].ToString());
                    dr["VASQuantity"] = OTSBD.clsStaticInfo.dbl(operationMaster["VASQuantity"].ToString());
                    dr["ArticleId"] = operationMaster["ArticleId"];

                    dr["IsAvgCT1"] = operationMaster["IsAvgCT1"].ToString();
                    dr["IsAvgCT2"] = operationMaster["IsAvgCT2"].ToString();
                    dr["IsAvgCT3"] = operationMaster["IsAvgCT3"].ToString();
                    dr["IsAvgCT4"] = operationMaster["IsAvgCT4"].ToString();
                    dr["IsAvgCT5"] = operationMaster["IsAvgCT5"].ToString();
                    dr["Remarks"] = operationMaster["Remarks"] == null ? "" : operationMaster["Remarks"].ToString();
                    dr["VasDescription"] = bplib.clsWebLib.RetValidLen(operationMaster["VasDescription"]);
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    operationMaster["Version"] = dr["Version"];

                    dr.EndEdit();
                }

                #endregion VASMaster

                #region vasChild

                con.getDataSet("Select * from [MST].[VASChild] where VASMasterID='" + operationMaster["Id"].ToString() + "'", out DataSet dsVasChild);
                //if (dtTime == null || dtTime.Rows.Count == 0)
                //{
                while (dsVasChild.Tables[0].DefaultView.Count > 0)
                {
                    dsVasChild.Tables[0].DefaultView[0].Delete();
                }
                //}

                //for (int i = 0; i < dsVasChild.Tables[0].Rows.Count; i++)
                //{
                //    dtTime.DefaultView.RowFilter = "ElementID='" + dsVasChild.Tables[0].Rows[i]["ElementID"].ToString() + "'";
                //    if (dtTime.DefaultView.Count == 0)
                //        dsVasChild.Tables[0].Rows[i].Delete();
                //}

                string ChildId = "";
                for (int i = 0; i < dtTime.Rows.Count; i++)
                {
                    //dsVasChild.Tables[0].DefaultView.RowFilter = "ElementID='" + dtTime.Rows[i]["ElementID"].ToString() + "'";
                    //if (dsVasChild.Tables[0].DefaultView.Count == 0)
                    //{
                    //addnew

                    if (OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT1"].ToString()) == 0
                        && OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT2"].ToString()) == 0
                           && OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT3"].ToString()) == 0
                              && OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT4"].ToString()) == 0
                                 && OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT5"].ToString()) == 0)
                        continue;

                    if (ChildId == "")
                    {
                        _genId.GenID("VasChild", out ChildId);
                        ChildId = "VASC-" + VasMasterId;
                    }
                    DataRow dr = dsVasChild.Tables[0].NewRow();
                    dr["Id"] = ChildId + "-" + (i + 1).ToString();
                    dr["VASMasterID"] = VasMasterId;
                    dr["OperationVariationSystemId"] = operationMaster["OperationVariationSystemId"].ToString();
                    dr["Sequence"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["Sequence"].ToString());
                    dr["ElementID"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["ElementID"].ToString());
                    dr["ElementTypeId"] = dtTime.Rows[i]["ElementTypeId"].ToString();
                    dr["TMU"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["TMU"].ToString());
                    dr["CT1"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT1"].ToString());
                    dr["CT2"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT2"].ToString());
                    dr["CT3"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT3"].ToString());
                    dr["CT4"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT4"].ToString());
                    dr["CT5"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT5"].ToString());
                    dr["TimeAvg"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["TimeAvg"].ToString());
                    dr["Ratings"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["Ratings"].ToString());
                    dr["BasicTime"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["BasicTime"].ToString());
                    dr["Version"] = OTSBD.clsStaticInfo.dbl(operationMaster["Version"].ToString());
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsVasChild.Tables[0].Rows.Add(dr);

                    //}
                    //else
                    //{
                    //    DataRow dr = dsVasChild.Tables[0].Rows[i];
                    //    dr.BeginEdit();

                    //    dr["VASMasterID"] = operationMaster["Id"].ToString();
                    //    dr["OperationVariationSystemId"] = operationMaster["OperationVariationSystemId"].ToString();
                    //    dr["Sequence"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["Sequence"].ToString());
                    //    dr["ElementID"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["ElementID"].ToString());
                    //    dr["ElementTypeId"] = dtTime.Rows[i]["ElementTypeId"].ToString();
                    //    dr["TMU"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["TMU"].ToString());
                    //    dr["CT1"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT1"].ToString());
                    //    dr["CT2"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT2"].ToString());
                    //    dr["CT3"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT3"].ToString());
                    //    dr["CT4"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT4"].ToString());
                    //    dr["CT5"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["CT5"].ToString());
                    //    dr["TimeAvg"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["TimeAvg"].ToString());
                    //    dr["Ratings"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["Ratings"].ToString());
                    //    dr["BasicTime"] = OTSBD.clsStaticInfo.dbl(dtTime.Rows[i]["BasicTime"].ToString());
                    //    dr["Version"] = OTSBD.clsStaticInfo.dbl(operationMaster["Version"].ToString());
                    //    dr["UpdatedBy"] = identity.Name;
                    //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    //    dr["UpdatedFromIP"] = identity.IPAddress;

                    //    dr.EndEdit();
                    //}
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsVasMaster, dsVasChild);
                #endregion vasChild


                return Json(new { Error = false, Id = VasMasterId, Version = operationMaster["Version"].ToString(), Message = "Data Save successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private DataTable CreateEmptyDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[12] {
                        new DataColumn("Sequence", typeof(int)),
                        new DataColumn("ElementID", typeof(int)),
                        new DataColumn("ElementTypeId",typeof(string)),
                        new DataColumn("CT1",typeof(decimal)),
                        new DataColumn("CT2",typeof(decimal)),
                        new DataColumn("CT3",typeof(decimal)),
                        new DataColumn("CT4",typeof(decimal)),
                        new DataColumn("CT5",typeof(decimal)),
                        new DataColumn("TimeAvg",typeof(decimal)),
                        new DataColumn("Ratings",typeof(int)),
                        new DataColumn("BasicTime",typeof(decimal)),
                        new DataColumn("TMU",typeof(decimal))

        });
            return dt;
        }

        private void ConvertModelToDataTable(string[] vasTimeRowDetails, Dictionary<string, object> _dbModel, DataTable ElementCode, int Sequence)
        {
            if (vasTimeRowDetails.Length == 12 && vasTimeRowDetails[1].ToString() != string.Empty)
            {
                ElementCode.DefaultView.RowFilter = "Code='" + vasTimeRowDetails[1] + "'";

                int ElementID = Convert.ToInt32(ElementCode.DefaultView[0]["Id"].ToString());
                string ElementTypeId = vasTimeRowDetails[2].ToString();
                decimal? CT1 = null;
                decimal? CT2 = null;
                decimal? CT3 = null;
                decimal? CT4 = null;
                decimal? CT5 = null;
                decimal? TimeAvg = null;
                int? Ratings = null;
                decimal? BasicTime = null;
                decimal? TMU = null;
                if (vasTimeRowDetails[3] != null && vasTimeRowDetails[3] != string.Empty)
                {
                    CT1 = Convert.ToDecimal(clsStaticInfo.dbl(vasTimeRowDetails[3]));
                }
                if (vasTimeRowDetails[4] != null && vasTimeRowDetails[4] != string.Empty)
                {
                    CT2 = Convert.ToDecimal(clsStaticInfo.dbl(vasTimeRowDetails[4]));
                }
                if (vasTimeRowDetails[5] != null && vasTimeRowDetails[5] != string.Empty)
                {
                    CT3 = Convert.ToDecimal(clsStaticInfo.dbl(vasTimeRowDetails[5]));
                }
                if (vasTimeRowDetails[6] != null && vasTimeRowDetails[6] != string.Empty)
                {
                    CT4 = Convert.ToDecimal(clsStaticInfo.dbl(vasTimeRowDetails[6]));
                }
                if (vasTimeRowDetails[7] != null && vasTimeRowDetails[7] != string.Empty)
                {
                    CT5 = Convert.ToDecimal(clsStaticInfo.dbl(vasTimeRowDetails[7]));
                }
                if (vasTimeRowDetails[8] != null && vasTimeRowDetails[8] != string.Empty)
                {
                    TimeAvg = Convert.ToDecimal(clsStaticInfo.dbl(vasTimeRowDetails[8]));
                }
                if (vasTimeRowDetails[9] != null && vasTimeRowDetails[9] != string.Empty)
                {
                    Ratings = Convert.ToInt32(vasTimeRowDetails[9]);
                }
                if (vasTimeRowDetails[10] != null && vasTimeRowDetails[10] != string.Empty)
                {
                    BasicTime = Convert.ToDecimal(vasTimeRowDetails[10]);
                }
                if (vasTimeRowDetails[11] != null && vasTimeRowDetails[11] != string.Empty)
                {
                    TMU = Convert.ToDecimal(vasTimeRowDetails[11]);
                }

                if (string.IsNullOrEmpty(ElementTypeId))
                {
                    throw new Exception("Please Select ElementType For Element " + ElementID + "..!");
                }
                DataRow dr = dtTime.NewRow();
                dr["Sequence"] = Sequence;
                dr["ElementID"] = ElementID;
                dr["ElementTypeId"] = ElementTypeId;
                dr["CT1"] = clsStaticInfo.dbl(CT1);
                dr["CT2"] = clsStaticInfo.dbl(CT2);
                dr["CT3"] = clsStaticInfo.dbl(CT3);
                dr["CT4"] = clsStaticInfo.dbl(CT4);
                dr["CT5"] = clsStaticInfo.dbl(CT5);
                dr["TimeAvg"] = clsStaticInfo.dbl(TimeAvg);
                dr["Ratings"] = clsStaticInfo.dbl(Ratings);
                dr["BasicTime"] = clsStaticInfo.dbl(BasicTime);
                dr["TMU"] = clsStaticInfo.dbl(TMU);

                dtTime.Rows.Add(dr);
            }
        }
        #endregion
    }
}