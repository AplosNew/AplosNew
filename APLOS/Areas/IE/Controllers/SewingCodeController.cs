using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.IE.Controllers
{
    public class SewingCodeController : Controller
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;
        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;

        public SewingCodeController(
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
        // GET: IE/SewingCode
        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region Forhad Code
        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            //sql = @"SELECT Id,Code,ShortName,StandardName,UserName,Description,TMU,SPI,RPM,LengthInCM,
            //CASE WHEN MCHand='1.00' THEN 'N(A straight burst on a single ply)'
            //  WHEN MCHand='1.10' THEN 'L(A straight, non-visible seam (ie, not having an appreciable affect on the final appearance of the product).)'
            //  WHEN MCHand='1.20' THEN 'M(A straight visible seam or a curved non visible seam.)'
            //  WHEN MCHand='1.30' THEN 'H(A curved visible seam or a seam worked in a confined space)' END MCHand,
            //CASE WHEN StoppingAccuracy='0' THEN 'A(Stop Approx)'
            //  WHEN StoppingAccuracy='9' THEN 'B(Stop Accurate)'
            //  WHEN StoppingAccuracy='17' THEN 'C(Stop Point)'
            //  END StoppingAccuracy,Activity,Element
            //FROM HKP.ElementCode WHERE CodeType='Additional Element Code' ORDER BY Code";

            sql = @"SELECT EC.Id,EC.Code,EC.ShortName,EC.StandardName,EC.UserName,EC.Description,EC.TMU,EC.SPI,EC.RPM,EC.LengthInCM,
                            EC.Activity,EC.Element,ec.StopAccuracyId, ec.HandlingFactorId,
                                      CONCAT( HF.Code,'(',hf.[Description],')') AS MCHand,
                                         CONCAT( SA.Code,'(',SA.[Description],')') AS StoppingAccuracy
                                        FROM HKP.ElementCode  EC
			                            LEFT JOIN AddtionalElementCodeHandlingFactor AS HF ON hf.Id=ec.HandlingFactorId
			                            LEFT JOIN AddtionalElementCodeStopAccuracy AS SA ON sa.Id=ec.StopAccuracyId
                            WHERE CodeType='Additional Element Code' ORDER BY Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBasicSettings()
        {

            string sql0 = @"SELECT * FROM AddtionalElementCodeSettings";

            string sql1 = @"SELECT * FROM AddtionalElementCodeStopAccuracy";

            string sql2 = @"SELECT  Id, Code, DegreeOfDifficulty, [Description], (1+ (AdditionRate/100)) AS AdditionalRate
                                FROM AddtionalElementCodeHandlingFactor";

            return Json(new
            {
                CS = _sqlRepository.GetDataCollection(sql0),
                SA = _sqlRepository.GetDataCollection(sql1),
                HF = _sqlRepository.GetDataCollection(sql2)
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult GetSelectedData(int Id)
        {
            string sql = "";
            sql = @"SELECT *
                    FROM HKP.ElementCode WHERE CodeType='Additional Element Code' AND Id='" + Id.ToString() + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveData(Dictionary<string, object> elementType)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                int Id = 0;
                con.getDataSet("SELECT * FROM [HKP].[ElementCode] WHERE Code='" + elementType["Code"].ToString() + "' AND Id<>'" + elementType["Id"].ToString() + "'", out DataSet dsElementType);
                if (dsElementType.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                con = new ConnectionManager.clsConnection();
                con.getDataSet("SELECT * FROM [HKP].[ElementCode] WHERE CodeType='Additional Element Code' AND Id='" + elementType["Id"].ToString() + "'", out dsElementType);
                if (dsElementType.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsElementType.Tables[0].NewRow();
                    dr["Code"] = elementType["Code"].ToString();
                    dr["ShortName"] = elementType["ShortName"].ToString();
                    dr["StandardName"] = elementType["StandardName"].ToString();
                    dr["UserName"] = elementType["UserName"].ToString();
                    dr["Description"] = elementType["Description"].ToString();
                    dr["TMU"] = OTSBD.clsStaticInfo.dbl(elementType["TMU"].ToString());
                    dr["SPI"] = elementType["SPI"].ToString();
                    dr["RPM"] = elementType["RPM"].ToString();
                    dr["NoOfStart"] = elementType["NoOfStart"].ToString();
                    dr["NoOfStop"] = elementType["NoOfStop"].ToString();
                    dr["LengthInCM"] = elementType["LengthInCM"].ToString();
                    dr["HandlingFactorId"] = elementType["HandlingFactorId"].ToString();
                    dr["StopAccuracyId"] = elementType["StopAccuracyId"].ToString();
                    dr["Activity"] = elementType["Activity"].ToString();
                    dr["Element"] = elementType["Element"].ToString();
                    dr["CodeType"] = "Additional Element Code";
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsElementType.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsElementType.Tables[0].Rows[0];
                    Id = Convert.ToInt32(dr["Id"]);
                    dr.BeginEdit();
                    dr["Code"] = elementType["Code"].ToString();
                    dr["ShortName"] = elementType["ShortName"].ToString();
                    dr["StandardName"] = elementType["StandardName"].ToString();
                    dr["UserName"] = elementType["UserName"].ToString();
                    dr["Description"] = elementType["Description"].ToString();
                    dr["TMU"] = OTSBD.clsStaticInfo.dbl(elementType["TMU"].ToString());
                    dr["SPI"] = elementType["SPI"].ToString();
                    dr["RPM"] = elementType["RPM"].ToString();
                    dr["NoOfStart"] = elementType["NoOfStart"].ToString();
                    dr["NoOfStop"] = elementType["NoOfStop"].ToString();
                    dr["LengthInCM"] = elementType["LengthInCM"].ToString();
                    dr["HandlingFactorId"] = elementType["HandlingFactorId"].ToString();
                    dr["StopAccuracyId"] = elementType["StopAccuracyId"].ToString();
                    dr["Activity"] = elementType["Activity"].ToString();
                    dr["Element"] = elementType["Element"].ToString();
                    dr["CodeType"] = "Additional Element Code";
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsElementType);

                return Json(new { Error = false, Message = _Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult DeleteSelectedData(int Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[ElementCode] WHERE CodeType='Additional Element Code' AND Id='" + Id.ToString() + "'");

                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        #endregion
    }
}