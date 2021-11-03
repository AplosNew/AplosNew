#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Model.Costings;
using Library.Service.Costings;
using Library.Data.Sql;
using System.Data;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using OTSBD;

#endregion

namespace Aplos.Areas.Costings.Controllers
{
    public class CostingComponentController : BaseController
    {
        #region Constructor
        private readonly ICostingComponentService _CostingComponentService;
        private readonly ISqlRepository _sqlRepository;
        private readonly string OperationComponentCode = "OPN";
        private readonly string CostingItemCMCode = "CM";
        private readonly string CostingItemUpChargeCode = "UPC";

        public CostingComponentController(ICostingComponentService costingComponentService, ISqlRepository repository)
        {
            _CostingComponentService = costingComponentService;
            _sqlRepository = repository;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            var sql = "SELECT Id AS [Value],cc.UserName AS [Text],cc.CostingSegment FROM hkp.CostingComponent AS cc";
            // return Json(new SelectList(_CostingComponentService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_CostingComponentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_CostingComponentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CostingComponent entity)
        {
            try
            {
                checkDuplicateComponent(entity);

                if (string.IsNullOrEmpty(entity.CalculationMethod))
                    entity.CalculationMethod = "FOB";

                _CostingComponentService.Insert(entity);
                return Json(new { Error = false, CostingSubCategory = entity, Sequence = _CostingComponentService.GetAutoSequence(), Message = AplosMessage.Success });

            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [HttpPost]
        public JsonResult saveDefaults()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                bplib.clsGenID genid = new bplib.clsGenID();
                string _id = "";
                DataSet dsComponent, dsItems;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("Select * from HKP.CostingComponent where Code IN ('" + OperationComponentCode + "','PRF','VLS')", out dsComponent);
                con.getDataSet("Select * from HKP.CostingItem where CostingComponentId IN (Select Id from HKP.CostingComponent where Code IN ('" + OperationComponentCode + "','PRF','VLS'))", out dsItems);

                dsComponent.Tables[0].DefaultView.RowFilter = "Code='" + OperationComponentCode + "'";
                if (dsComponent.Tables[0].DefaultView.Count == 0)
                {
                    genid.GenHRID(DateTime.Now.ToShortDateString().ToString(), "CostingComponent", out _id);
                    DataRow dr = dsComponent.Tables[0].NewRow();
                    _id = "S" + _id;
                    dr["Id"] = _id;
                    dr["Sequence"] = "1";
                    dr["Code"] = "OPN";
                    dr["ShortName"] = "OPN";
                    dr["StandardName"] = "Operation Cost";
                    dr["UserName"] = "Operation Cost / CM";
                    dr["Description"] = "Operation Cost";
                    dr["Remarks"] = "This component generally cosists with CM and Up Charge";
                    dr["Active"] = true;
                    dr["CostingSegment"] = "Operation";
                    dr["isSystemGenerated"] = true;
                    dr["CalculationMethod"] = "FOB";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsComponent.Tables[0].Rows.Add(dr);
                }
                else
                {
                    _id = dsItems.Tables[0].DefaultView[0]["Id"].ToString();
                }

                dsItems.Tables[0].DefaultView.RowFilter = "Code='" + CostingItemCMCode + "'";
                if (dsItems.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dsItems.Tables[0].NewRow();
                    dr["Id"] = _id + "1";
                    dr["Sequence"] = "1";
                    dr["Code"] = CostingItemCMCode;
                    dr["ShortName"] = "CM";
                    dr["StandardName"] = "CM";
                    dr["UserName"] = "CM";
                    dr["Description"] = "CM";
                    dr["Remarks"] = "This item has been auto generated by the system";
                    dr["Active"] = true;
                    dr["CostingComponentId"] = _id;
                    dr["isSystemGenerated"] = true;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsItems.Tables[0].Rows.Add(dr);
                }

                dsItems.Tables[0].DefaultView.RowFilter = "Code='" + CostingItemUpChargeCode + "'";
                if (dsItems.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dsItems.Tables[0].NewRow();
                    dr["Id"] = _id + "2";
                    dr["Sequence"] = "2";
                    dr["Code"] = CostingItemUpChargeCode;
                    dr["ShortName"] = "UPC";
                    dr["StandardName"] = "Up Charge";
                    dr["UserName"] = "Up Charge";
                    dr["Description"] = "Up Charge";
                    dr["Remarks"] = "This item has been auto generated by the system";
                    dr["Active"] = true;
                    dr["CostingComponentId"] = _id;
                    dr["isSystemGenerated"] = true;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsItems.Tables[0].Rows.Add(dr);
                }


                //For Value Loss

                dsComponent.Tables[0].DefaultView.RowFilter = "Code='VLS'";
                if (dsComponent.Tables[0].DefaultView.Count == 0)
                {
                    genid.GenHRID(DateTime.Now.ToShortDateString().ToString(), "CostingComponent", out _id);
                    DataRow dr = dsComponent.Tables[0].NewRow();
                    _id = "S" + _id;
                    dr["Id"] = _id;
                    dr["Sequence"] = "2";
                    dr["Code"] = "VLS";
                    dr["ShortName"] = "VLS";
                    dr["StandardName"] = "Value Loss";
                    dr["UserName"] = "Value Loss";
                    dr["Description"] = "Value Loss";
                    dr["Remarks"] = "This item has been auto generated by the system";
                    dr["Active"] = true;
                    dr["CostingSegment"] = "ValueLoss";
                    dr["isSystemGenerated"] = true;
                    dr["CalculationMethod"] = "FOB";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsComponent.Tables[0].Rows.Add(dr);
                }
                else
                {
                    _id = dsItems.Tables[0].DefaultView[0]["Id"].ToString();
                }

                dsItems.Tables[0].DefaultView.RowFilter = "Code='VLS'";
                if (dsItems.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dsItems.Tables[0].NewRow();
                    dr["Id"] = _id + "1";
                    dr["Sequence"] = "3";
                    dr["Code"] = "VLS";
                    dr["ShortName"] = "VLS";
                    dr["StandardName"] = "Rejections/Value Loss";
                    dr["UserName"] = "Rejections/Value Loss";
                    dr["Description"] = "Rejections/Value Loss";
                    dr["Remarks"] = "This item has been auto generated by the system";
                    dr["Active"] = true;
                    dr["CostingComponentId"] = _id;
                    dr["isSystemGenerated"] = true;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsItems.Tables[0].Rows.Add(dr);
                }
                //For profit

                dsComponent.Tables[0].DefaultView.RowFilter = "Code='PRF'";
                if (dsComponent.Tables[0].DefaultView.Count == 0)
                {
                    genid.GenHRID(DateTime.Now.ToShortDateString().ToString(), "CostingComponent", out _id);
                    DataRow dr = dsComponent.Tables[0].NewRow();
                    _id = "S" + _id;
                    dr["Id"] = _id;
                    dr["Sequence"] = "3";
                    dr["Code"] = "PRF";
                    dr["ShortName"] = "PRF";
                    dr["StandardName"] = "Profit";
                    dr["UserName"] = "Profit";
                    dr["Description"] = "Profit";
                    dr["Remarks"] = "This item has been auto generated by the system";
                    dr["Active"] = true;
                    dr["CostingSegment"] = "Profit";
                    dr["isSystemGenerated"] = true;
                    dr["CalculationMethod"] = "FOB";

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsComponent.Tables[0].Rows.Add(dr);
                }
                else
                {
                    _id = dsItems.Tables[0].DefaultView[0]["Id"].ToString();
                }

                dsItems.Tables[0].DefaultView.RowFilter = "Code='PROF'";
                if (dsItems.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dsItems.Tables[0].NewRow();
                    dr["Id"] = _id + "1";
                    dr["Sequence"] = "4";
                    dr["Code"] = "PROF";
                    dr["ShortName"] = "PROF";
                    dr["StandardName"] = "Profit";
                    dr["UserName"] = "Profit";
                    dr["Description"] = "Profit";
                    dr["Remarks"] = "This item has been auto generated by the system";
                    dr["Active"] = true;
                    dr["CostingComponentId"] = _id;
                    dr["isSystemGenerated"] = true;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsItems.Tables[0].Rows.Add(dr);
                }

                


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsComponent, dsItems);
            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
            return Json(new { Error = false, Sequence = _CostingComponentService.GetAutoSequence(), Message = AplosMessage.Success });

        }

        [HttpPost]
        public JsonResult Edit(CostingComponent entity)
        {
            checkDuplicateComponent(entity);
            _CostingComponentService.Update(entity);
            return Json(new { Sequence = _CostingComponentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        private void checkDuplicateComponent(CostingComponent entity)
        {
            try
            {
                string dupSql = "Select * from HKP.CostingComponent C where Id<>'" + entity.Id + "' AND (Code='" + entity.Code.Trim() + "' OR StandardName='" + entity.StandardName.Trim() + "')";
                if (_sqlRepository.GetDataTable(dupSql).Rows.Count > 0)
                    throw new System.Exception("Code or Standard name already exists");
            }
            catch (System.Exception ex)
            {

                throw (ex);
            }
        }

        public ActionResult Delete(string id)
        {
            var entity = _CostingComponentService.Find(id);
            _CostingComponentService.Delete(entity);
            return Json(new { Sequence = _CostingComponentService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion
    }
}