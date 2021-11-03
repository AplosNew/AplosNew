using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.IE.Controllers
{
    public class VASApprovalController : Controller
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;
        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;

        public VASApprovalController(
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
        // GET: IE/VASApproval
        #region -- Pages
        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Forhad Code
        [HttpPost, Authorize]
        public JsonResult GetOperationList(string FromDate, string ToDate)
        {
            string sql = "";
            sql = @"SELECT CONVERT(BIT,case when isnull(MAC.Id,'')<>'' THEN 0 else 1 end)AS Acknowledged, M.Id,M.OperationVariationSystemId,V.Code AS OperationCode,V.StandardName,
                    M.SPI,M.RPM,M.MachineAllowances,M.PersonalAllowances,M.AdditionalAllowances,ps.UserName AS ProductionSystem,M.FactorValue AS ProductionSystemAllowance,    OO.UserName as Operation,  M.VasDescription    
                   ,UPPER(APPR.ApprovedBy)ApprovedBy,convert(datetime, APPR.ApprovedDate)ApprovedDate,M.VASQuantity,
                    UPPER(M.AddedBy)AddedBy,convert(datetime, M.AddedDate)AddedDate,  v.TotalSAM AS OperationSAM,M.VASSAM,M.StandardSAM,O.Version,A.ApprovedVersion,TV.TotalVersion ,
                    mma.ShortName AS Machine,mmax.ShortName AS MachineActual,M.OriginalVideoName

                    FROM [MST].[VASMaster] M
                    INNER JOIN mst.VASMaster AS MV ON mv.OperationVariationSystemId=M.OperationVariationSystemId AND MV.Id=(SELECT TOP 1 Id FROM mst.VASMaster AS xm WHERE xm.OperationVariationSystemId=m.OperationVariationSystemId ORDER BY xm.[Version] DESC)
                        LEFT JOIN  [MST].[VASMaster] AS APPR ON  APPR.OperationVariationSystemId = M.OperationVariationSystemId AND m.Id=APPR.Id
				        AND APPR.Id=(SELECT TOP 1 Id FROM mst.VASMaster AS XV WHERE XV.OperationVariationSystemId = m.OperationVariationSystemId AND XV.IsApproved=1)

                    INNER JOIN [MST].[OperationVariation] V ON V.Id = M.OperationVariationSystemId 
                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=v.ArticleId
                    LEFT JOIN mst.MaterialMasterArticle AS mmax ON mmax.Id=M.ArticleId
                    LEFT JOIN [MST].[Operation] OO ON OO.Id = V.OperationId
                    LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=M.ProductionSystemId
                    INNER JOIN (SELECT OperationVariationSystemId,MAX(Version)Version FROM [MST].[VASMaster]
                        WHERE IsApproved IS NULL
	                    GROUP BY OperationVariationSystemId
                    )O ON O.OperationVariationSystemId = M.OperationVariationSystemId AND M.Version = O.Version
                    LEFT JOIN (
				    SELECT OperationVariationSystemId,Version ApprovedVersion 
				    FROM [MST].[VASMaster] WHERE IsApproved = 1
				    )A ON A.OperationVariationSystemId = M.OperationVariationSystemId
                    LEFT JOIN (SELECT OperationVariationSystemId,COUNT(*) TotalVersion 
					FROM MST.VASMaster
					GROUP BY OperationVariationSystemId) TV ON TV.OperationVariationSystemId = M.OperationVariationSystemId
					
					LEFT JOIN [MST].[VASMaster] MAC ON mac.OperationVariationSystemId=m.OperationVariationSystemId AND mac.Id=(SELECT TOP 1 Id FROM [MST].[VASMaster] xM WHERE xM.OperationVariationSystemId=m.OperationVariationSystemId AND ISNULL(xM.Acknowledged,0)=0)
                    
                     WHERE CAST(M.AddedDate AS DATE) BETWEEN '" + FromDate + @"'  AND '" + ToDate + @"'  AND isnull(M.Archive,0)=0
                    order by CONVERT(BIT,case when isnull(MAC.Id,'')<>'' THEN 0 else 1 end) ,v.Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetApprovedOperationList(string FromDate, string ToDate)
        {
            string sql = "";
            sql = @"SELECT CONVERT(BIT,case when isnull(MAC.Id,'')<>'' THEN 0 else 1 end)AS Acknowledged,
                    M.VasDescription,M.OperationVariationSystemId, M.Id,V.Code AS OperationCode,V.StandardName,
                    M.SPI,M.RPM,M.MachineAllowances,M.PersonalAllowances,mma.ShortName AS Machine,mmax.ShortName AS MachineActual,
                    UPPER(APPR.ApprovedBy)ApprovedBy,convert(datetime, APPR.ApprovedDate)ApprovedDate,
                    UPPER(M.AddedBy)AddedBy,convert(datetime, M.AddedDate)AddedDate,M.VASQuantity,
                     v.TotalSAM AS OperationSAM,M.VASSAM,M.StandardSAM,M.Version,TV.TotalVersion,M.AdditionalAllowances,
                        ps.UserName AS ProductionSystem,M.FactorValue AS ProductionSystemAllowance,M.OriginalVideoName,    O.UserName as Operation
					FROM [MST].[VASMaster] M
                    INNER JOIN [MST].[OperationVariation] V ON V.Id = M.OperationVariationSystemId
                        LEFT JOIN  [MST].[VASMaster] AS APPR ON  APPR.OperationVariationSystemId = M.OperationVariationSystemId AND m.Id=APPR.Id
				        AND APPR.Id=(SELECT TOP 1 Id FROM mst.VASMaster AS XV WHERE XV.OperationVariationSystemId = m.OperationVariationSystemId AND XV.IsApproved=1)

                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=v.ArticleId
                    LEFT JOIN mst.MaterialMasterArticle AS mmax ON mmax.Id=M.ArticleId
                    LEFT JOIN [MST].[Operation] O ON O.Id = V.OperationId
                    LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=M.ProductionSystemId
					INNER JOIN (SELECT OperationVariationSystemId,COUNT(*) TotalVersion 
					FROM MST.VASMaster
					GROUP BY OperationVariationSystemId) TV ON TV.OperationVariationSystemId = M.OperationVariationSystemId
					LEFT JOIN [MST].[VASMaster] MAC ON mac.OperationVariationSystemId=m.OperationVariationSystemId AND mac.Id=(SELECT TOP 1 Id FROM [MST].[VASMaster] xM WHERE xM.OperationVariationSystemId=m.OperationVariationSystemId AND ISNULL(xM.Acknowledged,0)=0)
 
                    WHERE CAST(APPR.ApprovedDate AS DATE) BETWEEN '" + FromDate + @"'  AND '" + ToDate + @"'  AND ISNULL(M.IsApproved,0) = 1  AND isnull(M.Archive,0)=0
                    order by CONVERT(BIT,case when isnull(MAC.Id,'')<>'' THEN 0 else 1 end),v.Code ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedOperation(string OperationVariationSystemId)
        {
            string sql = "";
            sql = @"SELECT  CONVERT(BIT,isnull(M.Acknowledged,0))AS Acknowledged,M.Id,M.OperationVariationSystemId,V.Code AS OperationCode,M.VasDescription,UPPER(M.ApprovedBy)ApprovedBy,convert(datetime, M.ApprovedDate)ApprovedDate,
                    UPPER(M.AddedBy)AddedBy,convert(datetime, M.AddedDate)AddedDate,
                    V.StandardName,M.SPI,M.RPM,M.MachineAllowances,M.PersonalAllowances,ps.UserName AS ProductionSystem,
                    mma.ShortName AS Machine,mmax.ShortName AS MachineActual, M.FactorValue AS ProductionSystemAllowance,        
                     v.TotalSAM AS OperationSAM,M.VASSAM,M.Version 
                    FROM [MST].[VASMaster] M
                    INNER JOIN [MST].[OperationVariation] V ON V.Id = M.OperationVariationSystemId
                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=v.ArticleId
                    LEFT JOIN mst.MaterialMasterArticle AS mmax ON mmax.Id=M.ArticleId
                    LEFT JOIN [MST].[Operation] O ON O.Id = V.OperationId
                    LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=M.ProductionSystemId
                    WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "'  AND isnull(M.Archive,0)=0";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetOperationVersion(string OperationVariationSystemId)
        {
            string sql = "";
            sql = @"SELECT  CONVERT(BIT,isnull(m.Acknowledged,0))AS Acknowledged,M.Id,M.OperationVariationSystemId,V.Code AS OperationCode,UPPER(APPR.ApprovedBy)ApprovedBy,convert(datetime, APPR.ApprovedDate)ApprovedDate,
                    UPPER(M.AddedBy)AddedBy,convert(datetime, M.AddedDate)AddedDate,V.StandardName,
                M.SPI,M.RPM,M.MachineAllowances,M.PersonalAllowances,M.AdditionalAllowances,ps.UserName AS ProductionSystem,
                mma.ShortName AS Machine,mmax.ShortName AS MachineActual, M.OriginalVideoName,  
                M.VasDescription,    M.FactorValue AS ProductionSystemAllowance,M.VASQuantity,
                 v.TotalSAM AS OperationSAM,M.StandardSAM,M.VASSAM,M.Version,APPR.Version AS ApprovedVersion
                FROM [MST].[VASMaster] M
                INNER JOIN [MST].[OperationVariation] V ON V.Id = M.OperationVariationSystemId
                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=v.ArticleId
                LEFT JOIN mst.MaterialMasterArticle AS mmax ON mmax.Id=M.ArticleId

                LEFT JOIN [MST].[Operation] O ON O.Id = V.OperationId
                LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=M.ProductionSystemId
				LEFT JOIN  [MST].[VASMaster] AS APPR ON  APPR.OperationVariationSystemId = M.OperationVariationSystemId AND m.Id=APPR.Id
				AND APPR.Id=(SELECT TOP 1 Id FROM mst.VASMaster AS XV WHERE XV.OperationVariationSystemId = m.OperationVariationSystemId AND XV.IsApproved=1)
				
                WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "'  AND isnull(M.Archive,0)=0  ORDER BY M.Version";

            var data = _sqlRepository.GetDataCollection(sql);
            //ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            //con.BeginTransaction();
            //con.executeQuery(@"UPDATE [MST].[VASMaster] SET Acknowledged = 1 WHERE OperationCode='" + OperationCode.ToString() + "'");
            //con.CommitTransaction();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedOperationTimeDetails(string VASMasterID)
        {
            string sql = "";
            sql = @"SELECT C.Id,VASMasterID,ElementID,E.ShortName AS ElementType,ECODE.Code AS ElementCode, 
                    CT1,CT2,CT3,CT4,CT5,TimeAvg,Ratings,BasicTime,Version,C.Sequence,C.TMU
                    FROM [MST].[VASChild] C
                    INNER JOIN HKP.ElementType E On E.Id = C.ElementTypeId
                    INNER JOIN HKP.ElementCode ECODE On ECODE.Id = C.ElementId
                    WHERE C.VASMasterID='" + VASMasterID + "'  ORDER BY C.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ApproveOperation(string Id, string OperationVariationSystemId)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {

                double VASSAM = 0;
                string VASSAMSource = "VASSAM";
                DataTable dt = _sqlRepository.GetDataTable("select * from mst.VasMaster where id='" + Id + "'");
                DataTable dtOV = _sqlRepository.GetDataTable("SELECT * FROM mst.OperationVariation AS ov where id='" + OperationVariationSystemId + "'");
                VASSAM = clsStaticInfo.dbl(dt.Rows[0]["VASSAM"].ToString());

                if (VASSAM > clsStaticInfo.dbl(dt.Rows[0]["StandardSAM"].ToString()))
                {
                    VASSAMSource = "StandardSAM";
                    VASSAM = clsStaticInfo.dbl(dt.Rows[0]["StandardSAM"].ToString());
                }

                if (VASSAM > clsStaticInfo.dbl(dtOV.Rows[0]["TotalSAM"].ToString()))
                {
                    VASSAMSource = "OperationSAM";
                    VASSAM = clsStaticInfo.dbl(dtOV.Rows[0]["TotalSAM"].ToString());
                }


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _Message = "";
                con.BeginTransaction();
                con.executeQuery(@"UPDATE [MST].[VASMaster] SET IsApproved = 0, Acknowledged = 1 WHERE OperationVariationSystemId='" + OperationVariationSystemId.ToString() + "' UPDATE [MST].[VASMaster] SET IsApproved = 1,WasApproved = 1 ,ApprovedBy= '" + identity.Name + "', ApprovedDate= '" + System.DateTime.Now.ToString() + "', ApprovedFromIP= '" + identity.IPAddress + "' WHERE Id= '" + Id + "'");
                con.executeQuery("UPDATE  mst.OperationVariation SET VASFINALSAM=" + VASSAM + ",VASSAMSOURCE='" + VASSAMSource + "',VASSAMApprovedBy='" + identity.Name + "',VASSAMApprovedDate='" + System.DateTime.Now.ToString() + "'  WHERE Id='" + OperationVariationSystemId + "'");
                con.CommitTransaction();

                _Message = "Approved Successfully..!";

                return Json(new { Error = false, Message = _Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult GetOperationVideoName(string OperationVariationSystemId, int Version)
        {
            string sql = "";
            sql = @"SELECT M.VASVideoName,M.OriginalVideoName
                FROM [MST].[VASMaster] M
                WHERE M.OperationVariationSystemId='" + OperationVariationSystemId + "' AND M.Version='" + Version + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}