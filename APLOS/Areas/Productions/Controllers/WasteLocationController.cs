using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.TaskScheduler.TaskScheduler;

namespace Aplos.Areas.Productions.Controllers
{
    public class WasteLocationController : BaseController
    {
        #region -- Constructor

        private readonly IMaterialStorageService _storageService;
        private readonly ISqlRepository _sqlRepository;
        public WasteLocationController(IMaterialStorageService storageService, ISqlRepository R)
        {
            _storageService = storageService;
            _sqlRepository = R;
        }

        #endregion -- Constructor

        #region Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations
        
        [HttpGet, Authorize]
        public ActionResult GetList(string companyId, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT * FROM [HKP].[MaterialStorage] WHERE CompanyGroupId='" + identity.CompanyGroupId + "' AND CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND Archive=0";

                return Json(_sqlRepository.GetDataCollection(sql,null), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data)
        {
            try
            {
                SaveData(data);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void SaveData(List<Dictionary<string, object>> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                foreach (var item in data)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                string mosql = "SELECT * FROM [HKP].[MaterialStorage] WHERE Id IN (" + id + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                foreach (var item in data)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["IsWasteLocation"] = item["IsWasteLocation"];
                       
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = System.DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion -- Operations
    }
}