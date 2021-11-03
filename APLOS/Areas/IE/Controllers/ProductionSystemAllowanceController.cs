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
    public class ProductionSystemAllowanceController : Controller
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;
        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;

        public ProductionSystemAllowanceController(
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
        // GET: IE/ProductionSystemAllowance
        #region -- Pages
        [Authorize, HttpGet]
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
            sql = @"SELECT BundleHandleTimeId, Factor, FactorValue FROM [HKP].[BundleHandleTime] ORDER BY Factor";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(int BundleHandleTimeId)
        {
            string sql = "";
            sql = @"SELECT BundleHandleTimeId, Factor, FactorValue FROM [HKP].[BundleHandleTime] WHERE BundleHandleTimeId='" + BundleHandleTimeId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> elementType)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                string _Message = "";
                int BundleHandleTimeId = 0;

                con.getDataSet("Select * from [HKP].[BundleHandleTime] where BundleHandleTimeId='" + elementType["BundleHandleTimeId"].ToString() + "'", out DataSet dsElementType);
                if (dsElementType.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsElementType.Tables[0].NewRow();
                    dr["Factor"] = elementType["Factor"].ToString();
                    dr["FactorValue"] = OTSBD.clsStaticInfo.dbl(elementType["FactorValue"].ToString()); ;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsElementType.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsElementType.Tables[0].Rows[0];
                    BundleHandleTimeId = Convert.ToInt32(dr["BundleHandleTimeId"]);
                    dr.BeginEdit();
                    dr["Factor"] = elementType["Factor"].ToString();
                    dr["FactorValue"] = elementType["FactorValue"].ToString();
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


        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(int BundleHandleTimeId)
        {
            try
            {
                if (string.IsNullOrEmpty(BundleHandleTimeId.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [HKP].[BundleHandleTime] WHERE BundleHandleTimeId='" + BundleHandleTimeId.ToString() + "'");

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