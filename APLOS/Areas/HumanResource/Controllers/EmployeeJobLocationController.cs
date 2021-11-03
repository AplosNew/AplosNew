using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Security.Core;
using Library.Service.Extension;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeJobLocationController : BaseController
    {
        #region Constructor
        clsEmployeeLoad clsEmployee = new clsEmployeeLoad();
        EmployeeProfile employeeProfile = new EmployeeProfile();
        private readonly ISqlRepository _sqlRepository;
        public EmployeeJobLocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsEmpJbLc = null;
                DataTable dtEmpJbLc = null;
                DataRow drEmpJbLc = null;
                DataView dvEmpJbLc = null;

                #region Employee JOB Location
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                OTSBD.IdentityParameter para = new OTSBD.IdentityParameter
                {
                    CompanyGroupId = identity.CompanyGroupId,
                    CompanyId = identity.CompanyId,
                    PlantId = identity.PlantId,
                    AddedBy = identity.Name,
                    AddedDate = DateTime.Now,
                    AddedFromIP = identity.IPAddress,
                    UpdatedBy = identity.Name,
                    UpdatedDate = DateTime.Now,
                    UpdatedFromIP = identity.IPAddress
                };

                string strEmpJbLcSystemID = "";


                clsEmployee.SaveEmpDateWiseJobLocation(data["EmpSystemID"].ToString(), strEmpJbLcSystemID, out dsEmpJbLc);
                dtEmpJbLc = dsEmpJbLc.Tables[0];
                dvEmpJbLc = new DataView();
                dvEmpJbLc.Table = dtEmpJbLc;
                dvEmpJbLc.RowFilter = "SystemID = '" + strEmpJbLcSystemID + "'";

          
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_JOB_LOC", out strEmpJbLcSystemID);
                strEmpJbLcSystemID = "J" + "-" + strEmpJbLcSystemID;

                clsEmployee.SaveEmpDateWiseJobLocation(data["EmpSystemID"].ToString(), strEmpJbLcSystemID, out dsEmpJbLc);
                dtEmpJbLc = dsEmpJbLc.Tables[0];
                dvEmpJbLc = new DataView();
                dvEmpJbLc.Table = dtEmpJbLc;
                dvEmpJbLc.RowFilter = "SystemID = '" + strEmpJbLcSystemID + "'";


                if (dvEmpJbLc.Count == 0)
                {// Add new block
                    drEmpJbLc = dtEmpJbLc.NewRow();
                    UpdateEmpDateWiseJobLocation("ADDNEW", strEmpJbLcSystemID,  data, para, ref drEmpJbLc);
                    dtEmpJbLc.Rows.Add(drEmpJbLc);
                }
                else
                {//edit block
                    drEmpJbLc = dvEmpJbLc[0].Row;
                    drEmpJbLc.BeginEdit();
                    UpdateEmpDateWiseJobLocation("EDIT", strEmpJbLcSystemID, data, para, ref drEmpJbLc);
                    drEmpJbLc.EndEdit();
                }
                dvEmpJbLc.RowFilter = null;

                #endregion Employee JOB Location

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsEmpJbLc);


                return Json(new { Error = false,Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        private void UpdateEmpDateWiseJobLocation(string OPN_FLAG, string strEmpJbLcSystemID, Dictionary<string, object> data, OTSBD.IdentityParameter para, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(strEmpJbLcSystemID.ToString().Trim());
                    drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(data["EmpSystemID"].ToString());

                    drLocal["AddedBy"] = para.AddedBy;
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["JobLcSystemID"] = bplib.clsWebLib.RetValidLen(data["JobLcSystemID"].ToString());
                drLocal["EffectiveDate"] = bplib.clsWebLib.DateData_AppToDB(data["EffectiveDate"], bplib.clsWebLib.DB_DATE_FORMAT);

                drLocal["UpdatedBy"] = para.UpdatedBy;
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
            }
        }//End Function

        #endregion -- Operations
    }
}