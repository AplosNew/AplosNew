#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Inventory;
using Library.Model.Inventory;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Products.Controllers
{
    public class MaterialRequsitionController : BaseController
    {
        #region Constructor
        private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterService;
        public MaterialRequsitionController(
              IMaterialRequsitionMasterServiceService materialRequsitionMasterService
            )
        {
            _materialRequsitionMasterService = materialRequsitionMasterService;
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
      
        //[Authorize,HttpPost]
        //public JsonResult Create(MaterialRequsitionMaster materialRequsition)
        //{
        //    _materialRequsitionMasterService.Insert(materialRequsition);
        //    return Json(new { MaterialRequsitionMaster = materialRequsition, Message = AplosMessage.Success });
        //}

        [Authorize, HttpPost]
        public JsonResult Create(MaterialRequsitionMaster materialRequsition)
        {

            try
            {
                saveData(materialRequsition);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MaterialRequsitionMaster), out sID);
            return sID;
        }

        private void saveData(MaterialRequsitionMaster data)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                
                string sql = "SELECT * FROM [TRN].[MaterialRequsitionMaster] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["EntityId"] = data.EntityId;
                    dr["RequisitionType"] = data.RequisitionType;


                    dr["RequirmentType"] = data.RequirmentType;
                    dr["NeedSpecialAppId"] = data.NeedSpecialAppId;
                    dr["AddedBy"] = identity.Name;

                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["RequisitionDate"] = data.RequisitionDate;
                    dr["Remarks"] = data.Remarks;

                    dr["CheckedBy"] = data.CheckedBy;
                    dr["CheckedByStatus"] = data.CheckedByStatus;
                    dr["AuthorizedBy"] = data.AuthorizedBy;
                    dr["AuthorizedByStatus"] = data.AuthorizedByStatus;
                    dr["IsApproved"] = data.IsApproved;
                    dr["RequisitionStatus"] = data.RequisitionStatus;

                    dsMaster.Tables[0].Rows.Add(dr);


                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["EntityId"] = data.EntityId;
                    dr["RequisitionType"] = data.RequisitionType;


                    dr["RequirmentType"] = data.RequirmentType;
                    dr["NeedSpecialAppId"] = data.NeedSpecialAppId;

                    dr["RequisitionDate"] = data.RequisitionDate;
                    dr["Remarks"] = data.Remarks;

                    dr["CheckedBy"] = data.CheckedBy;
                    dr["CheckedByStatus"] = data.CheckedByStatus;
                    dr["AuthorizedBy"] = data.AuthorizedBy;
                    dr["AuthorizedByStatus"] = data.AuthorizedByStatus;
                    dr["IsApproved"] = data.IsApproved;
                    dr["RequisitionStatus"] = data.RequisitionStatus;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                }
                               
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        [Authorize,HttpPost]
        public JsonResult Edit(MaterialRequsitionMaster materialRequsition)
        {
            _materialRequsitionMasterService.Update(materialRequsition);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _materialRequsitionMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}