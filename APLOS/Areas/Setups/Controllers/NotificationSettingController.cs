#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Employees;
using Library.Crosscutting.Security;
using System.Threading;
using System;
using Library.Service.Systems;
using System.Collections.Generic;
using Library.Service.Enums;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using OTSBD;
using System.Data;
using Library.Data.Repositories;
using Library.ViewModel.Setup;

#endregion

namespace Aplos.Areas.Setups.Controllers
{
    public class NotificationSettingController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<NotificationSetting> _baseRepository;
        public NotificationSettingController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
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

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string systemID = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "NotificationSetting", out idFromDB);
            systemID = idFromDB;
            sID = systemID.Trim();
            return sID;

        }

        [HttpGet]
        public ActionResult GetList(string plantId)
        {
            var sql = @"SELECT * FROM dbo.NotificationSetting Where PlantId='"+ plantId + "' Order By BusinessFlow	";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<NotificationSetting> entities)
        {
            try
            {
                SaveData(entities);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }

        private void SaveData(IEnumerable<NotificationSetting> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;

                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[NotificationSetting] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {

                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["Id"] = GetPK();

                            dr["PlantId"] = item.PlantId;
                            dr["BusinessFlow"] = item.BusinessFlow;
                            dr["NotificationAfterCreation"] = item.NotificationAfterCreation;
                            dr["RequiredChecking"] = item.RequiredChecking;
                            dr["NotificationAfterChecking"] = item.NotificationAfterChecking;
                            dr["RequiredApproval"] = item.RequiredApproval;
                            dr["NotificationAfterApproval"] = item.NotificationAfterApproval;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["BusinessFlow"] = item.BusinessFlow;
                            dr["NotificationAfterCreation"] = item.NotificationAfterCreation;
                            dr["RequiredChecking"] = item.RequiredChecking;
                            dr["NotificationAfterChecking"] = item.NotificationAfterChecking;
                            dr["RequiredApproval"] = item.RequiredApproval;
                            dr["NotificationAfterApproval"] = item.NotificationAfterApproval;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        
        #endregion


    }

    
    
}