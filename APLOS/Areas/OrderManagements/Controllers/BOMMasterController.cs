#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Data.Sql;
using System.Collections.Generic;
using Library.Data;
using Library.Service.Systems;
using Library.Model.Materials;
using Library.Service.Materials;
using System.IO;
using System.Web;
using Library.Service.Helpers;
using Aplos.Helpers;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class BOMMasterController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ICharacteristicsValueService _characteristicsValueService;
        public BOMMasterController(ISqlRepository R, IPKGeneratorService pkGeneratorService, ICharacteristicsValueService characteristicsValueService)
        {
            _sqlRepository = R;
            _pkGeneratorService = pkGeneratorService;
            _characteristicsValueService = characteristicsValueService;

        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetAttahedBoMInfo(string Id)
        {
            try
            {
                string sql = @"Select * from BOMMasterAttachmentWithItem  where BOMMasterId='" + Id + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetBOMSKUMappingDataForValidation(string BOMMasterId)
        {
            string sql = @"SELECT * FROM dbo.BOMSKUMapping WHERE BOMDetailId IN (SELECT Id FROM dbo.BOMDetail WHERE BOMMasterId='" + BOMMasterId + "' AND IsSKUCommon=0)";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var sql = @"SELECT MM.UserName FGMaterialMaster, MMA.StandardName FGArticle,A.*,MGM.UserName MaterialGroup,MM.WithSKU, PM.UserName AS ProductMasterName
                        ,AttachCount=(Select Count (Id) From BOMMasterAttachmentWithItem Where BOMMasterId=A.Id)		
                        FROM [dbo].[BOMMaster] A
                        LEFT JOIN MST.MaterialMaster MM ON MM.Id=A.FGMaterialMasterId
                        LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=A.FGArticleId
                        LEFT JOIN MST.MaterialGroupMaster MGM ON MGM.Id=MM.MaterialGroupMasterId
						LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
						LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        Order by A.AddedDate desc--CAST(A.Id AS int) desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string masterId)
        {
            var sql = @"SELECT B.*
                         ,MMD.Id RMMaterialMasterId,MMD.UserName RMMaterialMaster, MMAD.StandardName RMArticle,V.UserName PartyName, P.UserName Process, U.UserName UnitOfMeasurement
                         ,C1.UserName SKU1Name,C2.UserName SKU2Name,C3.UserName SKU3Name,CV1.UserName SKU1,CV2.UserName SKU2,CV3.UserName SKU3, SKUCommon=CASE WHEN B.IsSKUCommon=1 THEN 'Common' ELSE 'SKU Matrix' END,MMD.WithSKU
                        -- ,ISNULL(C1.ValueAssignmentLevel,A.ValueAssignmentLevel) C1ValueAssignmentLevel,ISNULL(C2.ValueAssignmentLevel,A.ValueAssignmentLevel) C2ValueAssignmentLevel,ISNULL(C3.ValueAssignmentLevel,A.ValueAssignmentLevel) C3ValueAssignmentLevel
                         ,B.Sequence Seq,B.AddedBy CreatedBy, FORMAT(B.AddedDate,'dd-MMM-yyyy') CreationDate
                         FROM [dbo].[BOMDetail] B
                        LEFT JOIN MST.MaterialMaster MMD ON MMD.Id=B.RMMaterialMasterId
                        LEFT JOIN MST.MaterialMasterArticle MMAD ON MMAD.Id=B.RMArticleId
                        LEFT JOIN HKP.Party V ON V.Id=B.VendorId
                        LEFT JOIN HKP.Process P ON P.Id=B.ProcessId
                        LEFT JOIN SCS.UnitOfMeasurement U ON U.Id=B.UoMId

                        LEFT JOIN HKP.Characteristics C1 ON C1.Id=B.FirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics C2 ON C2.Id=B.SecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics C3 ON C3.Id=B.ThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue CV1 ON CV1.Id=B.FirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue CV2 ON CV2.Id=B.SecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue CV3 ON CV3.Id=B.ThirdCharacteristicsValueId

						--LEFT JOIN (
					    --SELECT A.MaterialMasterId,B.ValueAssignmentLevel,DENSE_RANK() Over(Partition By A.MaterialMasterId Order By B.ValueAssignmentLevel) VAL
                        --   FROM [MST].[MaterialMasterCharacteristics] AS A
                        --    INNER JOIN [HKP].[Characteristics] AS B ON A.CharacteristicsId=B.Id  WHERE  A.MaterialMasterId IN(Select distinct RMMaterialMasterId FROM [dbo].[BOMDetail] WHERE BOMMasterId='" + masterId + @"')
						--) A ON A.MaterialMasterId=B.RMMaterialMasterId AND VAL=1
						WHERE B.BOMMasterId='" + masterId + "' ORDER BY B.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetValueAssignmentLevel(string MaterialMasterId)
        {
            return Json(_sqlRepository.GetDataCollection(@"SELECT A.Sequence, B.ValueAssignmentLevel
                           FROM [MST].[MaterialMasterCharacteristics] AS A
                            INNER JOIN [HKP].[Characteristics] AS B ON A.CharacteristicsId=B.Id 
							WHERE  A.MaterialMasterId='" + MaterialMasterId + "'"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllCharacteristicsValueByMaterial(string MaterialMasterId, string SP1, string SP2)
        {
            string sql = string.Empty;
            if (SP1 == "Specific" && SP2 == "General") 
            {
                sql = @"SELECT * FROM HKP.CharacteristicsValue WHERE SourceType='Specific' AND MaterialMasterId='"+ MaterialMasterId + @"'
                        UNION 
                        SELECT * FROM HKP.CharacteristicsValue WHERE SourceType='General'";
            }
            else if (SP1 == "General" && SP2 == "Specific")
            {
                sql = @"SELECT * FROM HKP.CharacteristicsValue WHERE SourceType='Specific' AND MaterialMasterId='" + MaterialMasterId + @"'
                        UNION 
                        SELECT * FROM HKP.CharacteristicsValue WHERE SourceType='General'";
            }
            else if (SP1 == "Specific")
            {
                sql = @"SELECT * FROM HKP.CharacteristicsValue WHERE SourceType='Specific' AND MaterialMasterId='"+ MaterialMasterId + "'";
            }
            else
            {
                sql = @"SELECT * FROM HKP.CharacteristicsValue WHERE SourceType='General'";
            }
            
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSavedCharacteristicsValueByMaterial(string BOMDetailId)
        {
            //return Json(_sqlRepository.GetDataCollection(@"Select B.* from BOMSKUMapping B
            //    LEFT JOIN hkp.CharacteristicsValue CV1 ON CV1.Id=B.RMFirstCharacteristicsValueId
            //    Where B.BOMDetailId='"+ BOMDetailId + @"' AND ISNULL(B.RMFirstCharacteristicsValueId,'')<>''
            //    UNION 
            //    Select B.* from BOMSKUMapping B
            //    LEFT JOIN hkp.CharacteristicsValue CV2 ON CV2.Id=B.RMSecondCharacteristicsValueId
            //    Where B.BOMDetailId='" + BOMDetailId + @"' AND ISNULL(B.RMSecondCharacteristicsValueId,'')<>''
            //    UNION 
            //    Select B.* from BOMSKUMapping B
            //    LEFT JOIN hkp.CharacteristicsValue CV3 ON CV3.Id=B.RMThirdCharacteristicsValueId
            //    Where B.BOMDetailId='" + BOMDetailId + @"' AND ISNULL(B.RMThirdCharacteristicsValueId,'')<>''"), JsonRequestBehavior.AllowGet);

            return Json(_sqlRepository.GetDataCollection(@"Select A.* from (
				Select B.RMFirstCharacteristicsValueId ValueId from BOMSKUMapping B
                LEFT JOIN hkp.CharacteristicsValue CV1 ON CV1.Id=B.RMFirstCharacteristicsValueId
                Where B.BOMDetailId='" + BOMDetailId + @"' AND ISNULL(B.RMFirstCharacteristicsValueId,'')<>''
                UNION ALL
                Select B.RMSecondCharacteristicsValueId ValueId from BOMSKUMapping B
                LEFT JOIN hkp.CharacteristicsValue CV2 ON CV2.Id=B.RMSecondCharacteristicsValueId
                Where B.BOMDetailId='" + BOMDetailId + @"' AND ISNULL(B.RMSecondCharacteristicsValueId,'')<>''
                UNION ALL
                Select B.RMThirdCharacteristicsValueId ValueId from BOMSKUMapping B
                LEFT JOIN hkp.CharacteristicsValue CV3 ON CV3.Id=B.RMThirdCharacteristicsValueId
                Where B.BOMDetailId='" + BOMDetailId + @"' AND ISNULL(B.RMThirdCharacteristicsValueId,'')<>''
				)A"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBOMSKUMappingList(string bomDetailId)
        {
            var sql = @"SELECT M.*,FGC1.StandardName FGSKU1, FGC2.StandardName FGSKU2,FGC3.StandardName FGSKU3,FGCV1.UserName FGSKU1Value,FGCV2.UserName FGSKU2Value,FGCV3.UserName FGSKU3Value
                        ,RMC1.StandardName RMSKU1, RMC2.StandardName RMSKU2,RMC3.StandardName RMSKU3,RMCV1.UserName RMSKU1Value,RMCV2.UserName RMSKU2Value,RMCV3.UserName RMSKU3Value
                        FROM [dbo].[BOMSKUMapping] M

                        LEFT JOIN HKP.Characteristics FGC1 ON FGC1.Id=M.FGFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics FGC2 ON FGC2.Id=M.FGSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics FGC3 ON FGC3.Id=M.FGThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue FGCV1 ON FGCV1.Id=M.FGFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue FGCV2 ON FGCV2.Id=M.FGSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue FGCV3 ON FGCV3.Id=M.FGThirdCharacteristicsValueId

                        LEFT JOIN HKP.Characteristics RMC1 ON RMC1.Id=M.RMFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC2 ON RMC2.Id=M.RMSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC3 ON RMC3.Id=M.RMThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue RMCV1 ON RMCV1.Id=M.RMFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV2 ON RMCV2.Id=M.RMSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV3 ON RMCV3.Id=M.RMThirdCharacteristicsValueId
                        WHERE M.BOMDetailId='" + bomDetailId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBOMSKUMappingListBySKU1(string bomDetailId, string characteristicsId)
        {
            var sql = @"SELECT M.*,FGC1.StandardName FGSKU1, FGC2.StandardName FGSKU2,FGC3.StandardName FGSKU3,FGCV1.UserName FGSKU1Value,FGCV2.UserName FGSKU2Value,FGCV3.UserName FGSKU3Value
                        ,RMC1.StandardName RMSKU1, RMC2.StandardName RMSKU2,RMC3.StandardName RMSKU3,RMCV1.UserName RMSKU1Value,RMCV2.UserName RMSKU2Value,RMCV3.UserName RMSKU3Value
                        FROM [dbo].[BOMSKUMapping] M

                        LEFT JOIN HKP.Characteristics FGC1 ON FGC1.Id=M.FGFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics FGC2 ON FGC2.Id=M.FGSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics FGC3 ON FGC3.Id=M.FGThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue FGCV1 ON FGCV1.Id=M.FGFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue FGCV2 ON FGCV2.Id=M.FGSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue FGCV3 ON FGCV3.Id=M.FGThirdCharacteristicsValueId

                        LEFT JOIN HKP.Characteristics RMC1 ON RMC1.Id=M.RMFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC2 ON RMC2.Id=M.RMSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC3 ON RMC3.Id=M.RMThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue RMCV1 ON RMCV1.Id=M.RMFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV2 ON RMCV2.Id=M.RMSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV3 ON RMCV3.Id=M.RMThirdCharacteristicsValueId
                        WHERE M.BOMDetailId='" + bomDetailId + "' AND M.FGFirstCharacteristicsId='" + characteristicsId + "'  AND ISNULL(M.RMFirstCharacteristicsId,'')<>''";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBOMSKUMappingListBySKU2(string bomDetailId, string characteristicsId)
        {
            var sql = @"SELECT M.*,FGC1.StandardName FGSKU1, FGC2.StandardName FGSKU2,FGC3.StandardName FGSKU3,FGCV1.UserName FGSKU1Value,FGCV2.UserName FGSKU2Value,FGCV3.UserName FGSKU3Value
                        ,RMC1.StandardName RMSKU1, RMC2.StandardName RMSKU2,RMC3.StandardName RMSKU3,RMCV1.UserName RMSKU1Value,RMCV2.UserName RMSKU2Value,RMCV3.UserName RMSKU3Value
                        FROM [dbo].[BOMSKUMapping] M

                        LEFT JOIN HKP.Characteristics FGC1 ON FGC1.Id=M.FGFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics FGC2 ON FGC2.Id=M.FGSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics FGC3 ON FGC3.Id=M.FGThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue FGCV1 ON FGCV1.Id=M.FGFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue FGCV2 ON FGCV2.Id=M.FGSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue FGCV3 ON FGCV3.Id=M.FGThirdCharacteristicsValueId

                        LEFT JOIN HKP.Characteristics RMC1 ON RMC1.Id=M.RMFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC2 ON RMC2.Id=M.RMSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC3 ON RMC3.Id=M.RMThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue RMCV1 ON RMCV1.Id=M.RMFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV2 ON RMCV2.Id=M.RMSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV3 ON RMCV3.Id=M.RMThirdCharacteristicsValueId
                        WHERE M.BOMDetailId='" + bomDetailId + "' AND M.FGSecondCharacteristicsId='" + characteristicsId + "' AND ISNULL(M.RMSecondCharacteristicsId,'')<>''";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BOMMaster entity)
        {
            SaveMasterData(entity, out string id);
            entity.Id = id;
            return Json(new { Data = entity, Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult CopyBOM(string Id)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment _attachment = new Library.OrderManagement.BOM.TemplateAttchment();
                _attachment.CopyBOMTemplate(Id);

                return Json(new { Error = false, Message = "BOM copied successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public JsonResult CopyBOMWithoutSKU(string Id)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment _attachment = new Library.OrderManagement.BOM.TemplateAttchment();
                _attachment.CopyBOMTemplateWithoutSKU(Id);

                return Json(new { Error = false, Message = "BOM copied successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CopyBomDetailData(string BOMMasterId, string Id)
        {
            try
            {
                Library.OrderManagement.BOM.TemplateAttchment _attachment = new Library.OrderManagement.BOM.TemplateAttchment();
                _attachment.CopyBOMTemplateDetail(BOMMasterId, Id);

                return Json(new { Error = false, Message = "BOM copied successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost]
        public JsonResult Edit(BOMMaster entity)
        {
            SaveMasterData(entity, out string id);
            entity.Id = id;
            return Json(new { Data = entity, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            DeleteBOM(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteBOM(string id)
        {
            string strSQL, strDCSQL, strDSQL, strDCSSQL, strSSQL, strDSSQL;
            string detailConsumptionIds = "";
            string bomDetailIds = "";
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                bomDetailIds = GetBomDetailIds(id);

                if (!string.IsNullOrEmpty(bomDetailIds))
                {
                    detailConsumptionIds = GetDetailConsumptionIds(bomDetailIds);
                }

                strDCSSQL = "DELETE FROM [dbo].[DetailConsumptionSKUMapping] Where DetailConsumptionId IN (" + detailConsumptionIds + ")";
                strDCSQL = "DELETE FROM [dbo].[DetailConsumption] Where BOMDetailId IN (" + bomDetailIds + ")";
                strSSQL = "DELETE FROM [dbo].[BOMSKUMapping] Where BOMDetailId IN (" + bomDetailIds + ")";
                strDSSQL = "DELETE FROM [dbo].[BOMDestination] Where BOMDetailId IN (" + bomDetailIds + ")";
                strDSQL = "DELETE FROM [dbo].[BOMDetail] WHERE BOMMasterId ='" + id + "'";
                strSQL = "DELETE FROM [dbo].[BOMMaster] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                if (!string.IsNullOrEmpty(detailConsumptionIds))
                {
                    objCon.ExecuteNonQueryWrapper(strDCSSQL, true, "1");
                }
                if (!string.IsNullOrEmpty(bomDetailIds))
                {
                    objCon.ExecuteNonQueryWrapper(strDCSQL, true, "1");
                    objCon.ExecuteNonQueryWrapper(strSSQL, true, "1");
                    objCon.ExecuteNonQueryWrapper(strDSSQL, true, "1");
                }
                objCon.ExecuteNonQueryWrapper(strDSQL, true, "1");
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

        [HttpPost]
        public ActionResult DeleteBomDetail(string id)
        {
            DeleteBomDetailData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteBomDetailData(string id)
        {
            string strSQL, strDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {


                strSQL = "DELETE FROM [dbo].[BOMDetail] WHERE Id = '" + id + "'";
                strDSQL = "DELETE FROM [dbo].[BOMDestination] WHERE BOMDetailId = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strDSQL, true, "1");
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
        public ActionResult DeleteDestination(string id)
        {
            DeleteDestinationData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        public void DeleteDestinationData(string id)
        {
            string strDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strDSQL = "DELETE FROM [dbo].[BOMDestination] WHERE BOMDetailId = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strDSQL, true, "1");
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

        string GetBomDetailIds(string masterId)
        {
            try
            {
                string ids = string.Empty;
                var _sql = @"Select Id from [dbo].[BOMDetail] Where BOMMasterId IN (Select Id from [dbo].BOMMaster Where id='" + masterId + "')";
                var list = _sqlRepository.GetDataCollection(_sql, null);
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        if (string.IsNullOrEmpty(ids))
                        {
                            ids = "'" + item["Id"] + "'";
                        }
                        else
                        {
                            ids += "," + "'" + item["Id"] + "'";
                        }
                    }
                    return ids;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        string GetDetailConsumptionIds(string detailIds)
        {
            try
            {
                string ids = string.Empty;
                //var list = null;
                List<Dictionary<string, object>> list = null;

                var _sql = @"Select Id from [dbo].[DetailConsumption] Where BOMDetailId IN (" + detailIds + ")";
                list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        if (string.IsNullOrEmpty(ids))
                        {
                            ids = "'" + item["Id"] + "'";
                        }
                        else
                        {
                            ids += "," + "'" + item["Id"] + "'";
                        }
                    }
                    return ids;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetGeneralPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BOM", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        private void SaveMasterData(BOMMaster data, out string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            id = string.Empty;
            try
            {
                if (data != null)
                {

                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    string sql = "SELECT * FROM [dbo].[BOMMaster] WHERE Id='" + data.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = GetGeneralPK();
                        dr["FGMaterialMasterId"] = data.FGMaterialMasterId;
                        dr["FGArticleId"] = data.FGArticleId;
                        dr["Description"] = data.Description;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["FGMaterialMasterId"] = data.FGMaterialMasterId;
                        dr["FGArticleId"] = data.FGArticleId;
                        dr["Description"] = data.Description;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                    id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private DataSet CheckCreateDetail(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM dbo.BOMDetail Where BOMMasterId='" + data["BOMMasterId"] + "' AND RMMaterialMasterId='" + data["RMMaterialMasterId"] + "' AND RMArticleId='" + data["RMArticleId"] + "' AND ProcessId='" + data["ProcessId"] + "' AND UoMId='" + data["UoMId"] + "' AND CustomerSpec='" + data["CustomerSpec"] + "' AND VendorSpec='" + data["VendorSpec"] + "' AND Description='" + data["Description"] + "' AND Id<>'" + data["Id"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        [HttpPost, Authorize]
        public JsonResult CreateDetail(Dictionary<string, object> data, string Destination)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataRow dr;

                DataSet dataSet = CheckCreateDetail(data);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This combination already taken.");
                }
                else
                {
                    DataSet dsMaster, dsDestination, dsBOMSKUMapping, dsCV;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BOMDetail WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BOMDestination Where BOMDetailId='" + data["Id"] + "'", out dsDestination, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BOMSKUMapping Where BOMDetailId='" + data["Id"] + "'", out dsBOMSKUMapping, false, "1");
                    con.OpenDataSetThroughAdapter("Select * from HKP.CharacteristicsValue Where MaterialMasterId='" + data["RMMaterialMasterId"] + "'", out dsCV, false, "1");

                    if (data["IsSKUCommon"].ToString() == "True")
                    {
                        if (dsBOMSKUMapping.Tables[0].Rows.Count > 0)
                        {
                            DeleteMatrixDataByDetailId(data["Id"].ToString());
                        }
                    }

                    string _Id = "";

                    #region data save update

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BOMDetail", out _Id);

                        data["Id"] = "BD" + _Id;
                        _Id = data["Id"].ToString();
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    #region destinations 

                    while (dsDestination.Tables[0].DefaultView.Count > 0)
                        dsDestination.Tables[0].DefaultView[0].Delete();
                    int count = 0;
                    if (Destination != null)
                    {
                        string[] destinations = Destination.Split(',');
                        foreach (string item in destinations)
                        {
                            dr = dsDestination.Tables[0].NewRow();
                            count++;
                            string pk = _Id + "_" + count;
                            dr["Id"] = pk;
                            dr["BOMDetailId"] = _Id;
                            dr["DestinationId"] = item;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsDestination.Tables[0].Rows.Add(dr);
                        }

                    }
                    #endregion Destination 

                    #endregion data save update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsDestination);

                    return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetBOMDestination(string BOMDetailId)
        {
            string sql = @"SELECT DestinationId FROM [dbo].[BOMDestination] Where BOMDetailId='" + BOMDetailId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAutoSequence(string BOMMasterId)
        {
            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[BOMDetail] Where BOMMasterId='" + BOMMasterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult UpdateMaterialSequence(List<string> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data changed!!!");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    con.executeQuery("UPDATE [BOMDetail] SET Sequence=" + (i + 1) + " where id='" + data[i] + "'");
                }

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Sequence updated successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private DataSet CheckBOMSKU1Mapping(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM dbo.BOMSKUMapping Where  Id<>'" + data["Id"] + "' AND BOMDetailId='" + data["BOMDetailId"] + "' AND ISNULL(FGFirstCharacteristicsId,'')='" + data["FGFirstCharacteristicsId"] + "' AND ISNULL(FGFirstCharacteristicsValueId,'')='" + data["FGFirstCharacteristicsValueId"] + "' AND ISNULL(RMFirstCharacteristicsId,'')='" + data["RMFirstCharacteristicsId"] + "' AND ISNULL(RMFirstCharacteristicsValueId,'')='" + data["RMFirstCharacteristicsValueId"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet CheckFGFirstCharacteristicsValue(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                //CmdText = @"SELECT * FROM dbo.BOMSKUMapping Where  Id<>'" + data["Id"] + "' AND BOMDetailId='" + data["BOMDetailId"] + "' AND ISNULL(FGFirstCharacteristicsId,'')='" + data["FGFirstCharacteristicsId"] + "' AND ISNULL(FGFirstCharacteristicsValueId,'')='" + data["FGFirstCharacteristicsValueId"] + "'"
                CmdText = @"SELECT * FROM dbo.BOMSKUMapping Where  Id='" + data["Id"] + "' AND BOMDetailId='" + data["BOMDetailId"] + "' AND ISNULL(FGFirstCharacteristicsId,'')='" + data["FGFirstCharacteristicsId"] + "' AND ISNULL(FGFirstCharacteristicsValueId,'')='" + data["FGFirstCharacteristicsValueId"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet CheckBOMSKU2Mapping(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM dbo.BOMSKUMapping Where  Id<>'" + data["Id"] + "' AND BOMDetailId='" + data["BOMDetailId"] + "' AND ISNULL(FGSecondCharacteristicsId,'')='" + data["FGSecondCharacteristicsId"] + "' AND ISNULL(FGSecondCharacteristicsValueId,'')='" + data["FGSecondCharacteristicsValueId"] + @"' AND ISNULL(RMSecondCharacteristicsId,'')='" + data["RMSecondCharacteristicsId"] + "' AND ISNULL(RMSecondCharacteristicsValueId,'')='" + data["RMSecondCharacteristicsValueId"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet CheckFGSecondCharacteristicsValue(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM dbo.BOMSKUMapping Where  Id<>'" + data["Id"] + "' AND BOMDetailId='" + data["BOMDetailId"] + "' AND ISNULL(FGSecondCharacteristicsId,'')='" + data["FGSecondCharacteristicsId"] + "' AND ISNULL(FGSecondCharacteristicsValueId,'')='" + data["FGSecondCharacteristicsValueId"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        [HttpPost, Authorize]
        public JsonResult CreateBOMSKU1Mapping(Dictionary<string, object> data, Dictionary<string, object> bomDetail)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var CheckFGFirstCharValue = CheckFGFirstCharacteristicsValue(data);
                //if (CheckFGFirstCharValue.Tables[0].Rows.Count > 0)
                //{
                //    throw new CustomException("This value already taken.");
                //}

                var checkProcess = CheckBOMSKU1Mapping(data);
                // var checkProcess = CheckBOMSKUMapping(data["Id"], data["FGFirstCharacteristicsId"], data["FGFirstCharacteristicsValueId"], data["FGSecondCharacteristicsId"], data["FGSecondCharacteristicsValueId"]);
                if (checkProcess.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This combination already taken.");

                }
                else
                {
                    DataSet dsMaster, dsbomDetail;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BOMSKUMapping WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BomDetail WHERE Id='" + bomDetail["Id"] + "'", out dsbomDetail, false, "1");

                    string _Id = "";

                    if (dsbomDetail.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsbomDetail.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["ConsumptionSpecificToSKU1"] = bomDetail["ConsumptionSpecificToSKU1"].ToString();
                        dr["ConsumptionSpecificToSKU2"] = bomDetail["ConsumptionSpecificToSKU2"].ToString();
                        dr["ConsumptionSpecificToSKU3"] = bomDetail["ConsumptionSpecificToSKU3"].ToString();

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }

                    #region data Insert update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BOMSKUMapping", out _Id);

                        data["Id"] = "BM" + _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data Insert update


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsbomDetail);


                    return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateBOMSKU2Mapping(Dictionary<string, object> data, Dictionary<string, object> bomDetail)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var CheckFGSecondCharValue = CheckFGSecondCharacteristicsValue(data);
                //if (CheckFGSecondCharValue.Tables[0].Rows.Count > 0)
                //{
                //    throw new CustomException("This value already taken.");
                //}

                var checkProcess = CheckBOMSKU2Mapping(data);
                // var checkProcess = CheckBOMSKUMapping(data["Id"], data["FGFirstCharacteristicsId"], data["FGFirstCharacteristicsValueId"], data["FGSecondCharacteristicsId"], data["FGSecondCharacteristicsValueId"]);
                if (checkProcess.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This combination already taken.");

                }
                else
                {
                    DataSet dsMaster, dsbomDetail;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BOMSKUMapping WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.BomDetail WHERE Id='" + bomDetail["Id"] + "'", out dsbomDetail, false, "1");

                    string _Id = "";

                    if (dsbomDetail.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsbomDetail.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["ConsumptionSpecificToSKU1"] = bomDetail["ConsumptionSpecificToSKU1"].ToString();
                        dr["ConsumptionSpecificToSKU2"] = bomDetail["ConsumptionSpecificToSKU2"].ToString();
                        dr["ConsumptionSpecificToSKU3"] = bomDetail["ConsumptionSpecificToSKU3"].ToString();

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }

                    #region data Insert update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BOMSKUMapping", out _Id);

                        data["Id"] = "BM" + _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }
                    #endregion data Insert update


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsbomDetail);


                    return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
                }

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


        [HttpPost, Authorize]
        public JsonResult DeleteMatrix(string id)
        {
            DeleteMatrixData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteMatrixData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[BOMSKUMapping] WHERE Id = '" + id + "'";
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
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        public void DeleteMatrixDataByDetailId(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[BOMSKUMapping] WHERE BOMDetailId = '" + id + "'";
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
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #region DetailConsumption
        [HttpGet, Authorize]
        public ActionResult GetDetailConsumptionSequence(string BOMDetailId)
        {
            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[DetailConsumption] Where BOMDetailId='" + BOMDetailId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult UpdatDetailConsumptionSequence(List<string> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data changed!!!");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                for (int i = 0; i < data.Count; i++)
                {
                    con.executeQuery("UPDATE [DetailConsumption] SET Sequence=" + (i + 1) + " where id='" + data[i] + "'");
                }

                con.CommitTransaction();

                return Json(new { Error = false, Message = "Sequence updated successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private DataSet CheckDetailConsumption(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM dbo.DetailConsumption Where BOMDetailId='" + data["BOMDetailId"] + "' AND RMMaterialMasterId='" + data["RMMaterialMasterId"] + "' AND RMArticleId='" + data["RMArticleId"] + "' AND ProcessId='" + data["ProcessId"] + "' AND UoMId='" + data["UoMId"] + "' AND CustomerSpec='" + data["CustomerSpec"] + "' AND VendorSpec='" + data["VendorSpec"] + "'  AND Description='" + data["Description"] + "' AND Id<>'" + data["Id"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        [HttpPost, Authorize]
        public JsonResult CreateDetailConsumption(Dictionary<string, object> data)
        {
            try
            {
                DataSet dataSet = CheckDetailConsumption(data);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This combination already taken.");
                }
                else
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.DetailConsumption WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DetailConsumption", out _Id);

                        data["Id"] = "DC" + _Id;
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

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailConsumptionList(string masterId)
        {
            var sql = @"SELECT B.*
                         ,MMD.UserName RMMaterialMaster, MMAD.StandardName RMArticle,V.UserName PartyName, P.UserName Process, U.UserName UnitOfMeasurement
                         ,C1.UserName SKU1Name,C2.UserName SKU2Name,C3.UserName SKU3Name,CV1.UserName SKU1,CV2.UserName SKU2,CV3.UserName SKU3, SKUCommon=CASE WHEN B.IsSKUCommon=1 THEN 'Specific' ELSE 'SKU Matrix' END,MMD.WithSKU
                         ,C1.ValueAssignmentLevel C1ValueAssignmentLevel,C2.ValueAssignmentLevel C2ValueAssignmentLevel,C3.ValueAssignmentLevel C3ValueAssignmentLevel
                         FROM [dbo].[DetailConsumption] B
                        LEFT JOIN MST.MaterialMaster MMD ON MMD.Id=B.RMMaterialMasterId
                        LEFT JOIN MST.MaterialMasterArticle MMAD ON MMAD.Id=B.RMArticleId
                        LEFT JOIN HKP.Party V ON V.Id=B.VendorId
                        LEFT JOIN HKP.Process P ON P.Id=B.ProcessId
                        LEFT JOIN SCS.UnitOfMeasurement U ON U.Id=B.UoMId

                        LEFT JOIN HKP.Characteristics C1 ON C1.Id=B.FirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics C2 ON C2.Id=B.SecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics C3 ON C3.Id=B.ThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue CV1 ON CV1.Id=B.FirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue CV2 ON CV2.Id=B.SecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue CV3 ON CV3.Id=B.ThirdCharacteristicsValueId
                        WHERE B.BOMDetailId='" + masterId + "' Order BY B.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteBomDetailConsumption(string id)
        {
            DeleteBomDetailConsumptionData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteBomDetailConsumptionData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[DetailConsumption] WHERE Id = '" + id + "'";

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

        #region  DetailConsumptionMatrix   

        private DataSet CheckDetailConsumptionSKUMapping(Dictionary<string, object> data)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM dbo.DetailConsumptionSKUMapping Where  Id<>'" + data["Id"] + "' AND DetailConsumptionId='" + data["DetailConsumptionId"] + "' AND ISNULL(RMFirstCharacteristicsId,'')='" + data["RMFirstCharacteristicsId"] + "' AND ISNULL(RMFirstCharacteristicsValueId,'')='" + data["RMFirstCharacteristicsValueId"] + "' AND ISNULL(RMSecondCharacteristicsId,'')='" + data["RMSecondCharacteristicsId"] + "' AND ISNULL(RMSecondCharacteristicsValueId,'')='" + data["RMSecondCharacteristicsValueId"] + @"'
                            AND ISNULL(SubFirstCharacteristicsId,'')='" + data["SubFirstCharacteristicsId"] + "' AND ISNULL(SubFirstCharacteristicsValueId,'')='" + data["SubFirstCharacteristicsValueId"] + "' AND ISNULL(SubSecondCharacteristicsId,'')='" + data["SubSecondCharacteristicsId"] + "' AND ISNULL(SubSecondCharacteristicsValueId,'')='" + data["SubSecondCharacteristicsValueId"] + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }

        [HttpPost, Authorize]
        public JsonResult CreateDetailConsumptionSKUMapping(Dictionary<string, object> data)
        {
            try
            {
                var checkProcess = CheckDetailConsumptionSKUMapping(data);
                if (checkProcess.Tables[0].Rows.Count > 0)
                {
                    throw new CustomException("This combination already taken.");

                }
                else
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[DetailConsumptionSKUMapping] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DetailConsumptionSKUMapping", out _Id);

                        data["Id"] = "DCS" + _Id;
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

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailConsumptionSKUMappingListBySKU1(string detailConsumptionId, string characteristicsId)
        {
            var sql = @"SELECT M.*,RMC1.StandardName RMSKU1, RMC2.StandardName RMSKU2,RMC3.StandardName RMSKU3,RMCV1.UserName RMSKU1Value,RMCV2.UserName RMSKU2Value,RMCV3.UserName RMSKU3Value
                        ,SUBC1.StandardName SUBSKU1, SUBC2.StandardName SUBSKU2,SUBC3.StandardName SUBSKU3,SUBCV1.UserName SUBSKU1Value,SUBCV2.UserName SUBSKU2Value,SUBCV3.UserName SUBSKU3Value
                        FROM [dbo].[DetailConsumptionSKUMapping] M

                        LEFT JOIN HKP.Characteristics RMC1 ON RMC1.Id=M.RMFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC2 ON RMC2.Id=M.RMSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC3 ON RMC3.Id=M.RMThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue RMCV1 ON RMCV1.Id=M.RMFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV2 ON RMCV2.Id=M.RMSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV3 ON RMCV3.Id=M.RMThirdCharacteristicsValueId

                        LEFT JOIN HKP.Characteristics SUBC1 ON SUBC1.Id=M.SubFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics SUBC2 ON SUBC2.Id=M.SubSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics SUBC3 ON SUBC3.Id=M.SubThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue SUBCV1 ON SUBCV1.Id=M.SUBFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue SUBCV2 ON SUBCV2.Id=M.SUBSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue SUBCV3 ON SUBCV3.Id=M.SUBThirdCharacteristicsValueId
                        WHERE M.DetailConsumptionId='" + detailConsumptionId + "' AND M.RMFirstCharacteristicsId='" + characteristicsId + "'   AND ISNULL(M.SubFirstCharacteristicsId,'')<>''";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailConsumptionSKUMappingListBySKU2(string detailConsumptionId, string characteristicsId)
        {
            var sql = @"SELECT M.*,RMC1.StandardName RMSKU1, RMC2.StandardName RMSKU2,RMC3.StandardName RMSKU3,RMCV1.UserName RMSKU1Value,RMCV2.UserName RMSKU2Value,RMCV3.UserName RMSKU3Value
                        ,SUBC1.StandardName SUBSKU1, SUBC2.StandardName SUBSKU2,SUBC3.StandardName SUBSKU3,SUBCV1.UserName SUBSKU1Value,SUBCV2.UserName SUBSKU2Value,SUBCV3.UserName SUBSKU3Value
                        FROM [dbo].[DetailConsumptionSKUMapping] M

                        LEFT JOIN HKP.Characteristics RMC1 ON RMC1.Id=M.RMFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC2 ON RMC2.Id=M.RMSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics RMC3 ON RMC3.Id=M.RMThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue RMCV1 ON RMCV1.Id=M.RMFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV2 ON RMCV2.Id=M.RMSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue RMCV3 ON RMCV3.Id=M.RMThirdCharacteristicsValueId

                        LEFT JOIN HKP.Characteristics SUBC1 ON SUBC1.Id=M.SubFirstCharacteristicsId
                        LEFT JOIN HKP.Characteristics SUBC2 ON SUBC2.Id=M.SubSecondCharacteristicsId
                        LEFT JOIN HKP.Characteristics SUBC3 ON SUBC3.Id=M.SubThirdCharacteristicsId

                        LEFT JOIN HKP.CharacteristicsValue SUBCV1 ON SUBCV1.Id=M.SUBFirstCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue SUBCV2 ON SUBCV2.Id=M.SUBSecondCharacteristicsValueId
                        LEFT JOIN HKP.CharacteristicsValue SUBCV3 ON SUBCV3.Id=M.SUBThirdCharacteristicsValueId
                        WHERE M.DetailConsumptionId='" + detailConsumptionId + "' AND M.RMSecondCharacteristicsId='" + characteristicsId + "'   AND ISNULL(M.SubSecondCharacteristicsId,'')<>''";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDetailConsumptionMatrix(string id)
        {
            DeleteDetailConsumptionMatrixData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteDetailConsumptionMatrixData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[DetailConsumptionSKUMapping] WHERE Id = '" + id + "'";
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
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #endregion DetailConsumptionMatrix


        [HttpPost,Authorize]
        public JsonResult CreateCharacteristicsValue(CharacteristicsValue entity, string MaterialMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.CompanyGroupId = identity.CompanyGroupId;
                _characteristicsValueService.InsertBOMSKU(entity);

                return Json(new { CharacteristicsValue = entity, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }



        #endregion Operations

        #region upload product picture

        [HttpGet, Authorize]
        public ActionResult GetBoMDocumentsData(string bomId)
        {
            var sql = @"SELECT * FROM [dbo].[BoMDocuments] WHERE BoMId='" + bomId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        private string GetIssueDocumentPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(BoMDocuments), out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);

                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["BoMId"].ToString()))
                    throw new Exception("Save the Issue Transaction first.");




                foreach (var file in UploadDefault)
                {

                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM dbo.BoMDocuments WHERE Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = file.FileName;
                        dsLocal.Tables[0].Rows[0]["Description"] = AdditionalData.Rows[0]["Description"].ToString();

                        dsLocal.Tables[0].Rows[0]["AddedBy"] = identity.Name;
                        dsLocal.Tables[0].Rows[0]["AddedFromIP"] = identity.IPAddress;
                        dsLocal.Tables[0].Rows[0]["AddedDate"] = System.DateTime.Now.ToString();

                        dsLocal.Tables[0].Rows[0]["UpdatedBy"] = identity.Name;
                        dsLocal.Tables[0].Rows[0]["UpdatedFromIP"] = identity.IPAddress;
                        dsLocal.Tables[0].Rows[0]["UpdatedDate"] = System.DateTime.Now.ToString();


                        dsLocal.Tables[0].Rows[0].EndEdit();

                        var fileName = Path.GetFileName(dsLocal.Tables[0].Rows[0]["Id"] + new FileInfo(file.FileName).Extension);
                        var destinationPath = Path.Combine(ResourcesPathReader.GetIssueTransactionDocumentsPath(), fileName);

                        if (System.IO.Directory.Exists(ResourcesPathReader.GetIssueTransactionDocumentsPath()) == false)
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(ResourcesPathReader.GetIssueTransactionDocumentsPath());
                            }
                            catch (Exception ex)
                            {

                            }
                        }


                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                    else
                    {
                        DataRow dr = dsLocal.Tables[0].NewRow();

                        dr["Id"] = AdditionalData.Rows[0]["BoMId"].ToString() + "-" + GetIssueDocumentPK();
                        dr["BoMId"] = AdditionalData.Rows[0]["BoMId"].ToString();
                        dr["FileName"] = file.FileName;
                        dr["Description"] = AdditionalData.Rows[0]["Description"].ToString();

                        dr["AddedBy"] = identity.EmployeeId;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dsLocal.Tables[0].Rows.Add(dr);

                        var fileName = Path.GetFileName(dr["Id"].ToString() + new FileInfo(file.FileName).Extension);
                        var destinationPath = Path.Combine(ResourcesPathReader.GetBoMDocPath(), fileName);

                        if (System.IO.Directory.Exists(ResourcesPathReader.GetBoMDocPath()) == false)
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(ResourcesPathReader.GetBoMDocPath());
                            }
                            catch (Exception ex)
                            {

                            }
                        }


                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);
                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [Authorize]
        public ActionResult RemoveDefault(string[] fileNames)
        {
            foreach (var fullName in fileNames)
            {
                var fileName = Path.GetFileName(fullName);
                var physicalPath = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            return Content("");
        }

        public Dictionary<string, object> GetFile(string systemId)
        {
            try
            {
                var sql = @"Select Id, FileName From [dbo].[BoMDocuments]  Where Id='" + systemId + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateDocuments(FormCollection form, HttpPostedFileBase[] file)
        {
            var issueTransactionDocuments = new JavaScriptSerializer().Deserialize<BoMDocuments>(form["BoMDocuments"]);

            SaveData(issueTransactionDocuments, out string docId);
            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetBoMDocPath();
                var path = Path.Combine(directory);

                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = GetFile(issueTransactionDocuments.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["Id"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != issueTransactionDocuments.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }

                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + docId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + docId + Path.GetExtension(item.FileName));
                    }
                }

            }
            return Json(new { Message = AplosMessage.Success });
        }

        private void SaveData(BoMDocuments data, out string docId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = "SELECT * FROM [dbo].[BoMDocuments] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = data.BoMId + "-" + GetIssueDocumentPK();
                    dr["BoMId"] = data.BoMId;
                    dr["FileName"] = data.FileName;
                    dr["Description"] = data.Description;

                    dr["AddedBy"] = identity.EmployeeId;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["FileName"] = data.FileName;
                    dr["Description"] = data.Description;
                    dr["BoMId"] = data.BoMId;

                    dr["UpdatedBy"] = identity.EmployeeId;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                docId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDocument(string id)
        {
            var directory = ResourcesPathReader.GetBoMDocPath();
            var path = Path.Combine(directory);
            var fileId = "";
            var fileName = "";
            var data = GetFile(id);
            if (data.Count > 0)
            {
                if (!string.IsNullOrEmpty(data["Id"].ToString()) &&
                !string.IsNullOrEmpty(data["FileName"].ToString()))
                    fileId = data["Id"].ToString();
                fileName = data["FileName"].ToString();
                if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                    System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = "DELETE FROM [dbo].[BoMDocuments] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        #endregion upload product picture
    }
    public class BOMMaster : BaseModel
    {
        public string Id { get; set; }
        public string FGMaterialMasterId { get; set; }
        public string FGArticleId { get; set; }
        public string Description { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class BOMDestination
    {
        public string Id { get; set; }
        public string BOMDetailId { get; set; }
        public string DestinationId { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class BoMDocuments
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }

        public string BoMId { get; set; }
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

        #region Navigation Properties


        #endregion Navigation Properties
    }
}