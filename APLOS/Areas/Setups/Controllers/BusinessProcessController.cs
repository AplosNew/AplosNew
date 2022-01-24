#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Security.Core;
using Library.Service.Setups;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class BusinessProcessController : BaseController
    {
        #region Constructor

        private readonly IBusinessProcessService _brandService;
        private readonly ISqlRepository _sqlRepository;
        public BusinessProcessController(IBusinessProcessService brandService, ISqlRepository R)
        {
            _brandService = brandService;
            _sqlRepository = R;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_brandService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBusinessProcessList(string materialMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_brandService.GetBusinessProcessList(identity.CompanyGroupId, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BusinessProcess BusinessProcess)
        {
            _brandService.Insert(BusinessProcess);
            return Json(new { BusinessProcess, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BusinessProcess BusinessProcess)
        {
            _brandService.Update(BusinessProcess);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _brandService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet,Authorize]
        public JsonResult GetDynamicColList(string businessProcessId)
        {
            try
            {
               string  sql= @"SELECT * FROM dbo.FabricRollManagementColSetting Where ISNULL(BusinessProcessId,'"+businessProcessId+ "')='" + businessProcessId + "'";
                return Json(_sqlRepository.GetDataCollection(sql),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
              throw ex;
            }
        }

        [HttpPost]
        public JsonResult SaveBPSatting(List<Dictionary<string, object>> funds, string BusinessProcessId)
        {
            try
            {
              
                SaveData(funds, BusinessProcessId);


                return Json(new {Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FabricRollManagementColSetting", out sID);
            return sID;
        }

        private void SaveData(List<Dictionary<string, object>> funds, string BusinessProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsMasterOrder, dsfunds;
            string contId = string.Empty;
            string id = string.Empty;
            try
            {
                
                #region FUND 

                DataSet dsChild;
               
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.FabricRollManagementColSetting where  BusinessProcessId='" + BusinessProcessId + "'", out dsChild, false, "1");
                #region data update

                if (funds != null)
                {
                    foreach (var item in funds)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetPK();
                            item["BusinessProcessId"] = BusinessProcessId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
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

    }
}