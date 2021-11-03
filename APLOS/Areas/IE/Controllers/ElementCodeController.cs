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
    public class ElementCodeController : Controller
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;
        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;

        public ElementCodeController(
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
        // GET: IE/ElementCode
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
            sql = @"SELECT Id,Code,ShortName,StandardName,UserName,Description,TMU,MCHand,Activity,Element FROm [HKP].[ElementCode] WHERE CodeType='Element'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(int Id)
        {
            string sql = "";
            sql = @"SELECT Id,Code,ShortName,StandardName,UserName,Description,TMU,MCHand,Activity,Element FROM [HKP].[ElementCode] WHERE Id='" + Id + "'";

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

                con.getDataSet("Select * from [HKP].[ElementCode] where Id='" + elementType["Id"].ToString() + "'", out DataSet dsElementType);
                if (dsElementType.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsElementType.Tables[0].NewRow();
                    dr["Code"] = elementType["Code"].ToString();
                    dr["ShortName"] = elementType["ShortName"].ToString();
                    dr["StandardName"] = elementType["StandardName"].ToString();
                    dr["UserName"] = elementType["UserName"].ToString();
                    dr["Activity"] = elementType["Activity"].ToString();
                    dr["Element"] = elementType["Element"].ToString();
                    dr["Description"] = elementType["Description"].ToString();                    
                    dr["TMU"] = OTSBD.clsStaticInfo.dbl(elementType["TMU"].ToString());
                    dr["MCHand"] = elementType["MCHand"].ToString();
                    dr["CodeType"] = "Element";
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
                    dr["Activity"] = elementType["Activity"].ToString();
                    dr["Element"] = elementType["Element"].ToString();
                    dr["Description"] = elementType["Description"].ToString();
                    dr["TMU"] = OTSBD.clsStaticInfo.dbl(elementType["TMU"].ToString());
                    dr["MCHand"] = elementType["MCHand"].ToString();
                    dr["CodeType"] = "Element";
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
                con.executeQuery("DELETE FROM [HKP].[ElementCode] WHERE Id='" + Id.ToString() + "'");

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