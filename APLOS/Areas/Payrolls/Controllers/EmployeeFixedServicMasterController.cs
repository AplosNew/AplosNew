using Aplos.Controllers;
using Aplos.Properties;
using ConnectionManager.DAL;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class EmployeeFixedServicMasterController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private ConManager objCon;

        public EmployeeFixedServicMasterController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead+' ['+HeadType+']' as UserName 
                            from [dbo].[SalaryHead] 
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult getSalaryHeadWiseAmountSettinglist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select shas.SalaryHeadId,sh.SalaryHead,shas.ServicComponent,shas.DurationType,shas.Id
                            From EmployeeFixedServiceMaster as shas
                            LEFT JOIN SalaryHead sh on sh.SalaryHeadId=shas.SalaryHeadId 
                            where  shas.CompanyGroupId='" + identity.CompanyGroupId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Save(EmployeeFixedServiceMaster SalaryHeadWiseAmountSetting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SaveCompliance(SalaryHeadWiseAmountSetting);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void SaveCompliance(EmployeeFixedServiceMaster SalaryHeadWiseAmountSetting)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsMasterV;
            try
            {

                string sqlV = "SELECT * FROM EmployeeFixedServiceMaster where ServicComponent='"+ SalaryHeadWiseAmountSetting.ServicComponent + @"' and DurationType='" + SalaryHeadWiseAmountSetting.DurationType + @"' and CompanyGroupId='" + identity.CompanyGroupId + "' AND Id !='" + SalaryHeadWiseAmountSetting.Id + "' ";;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlV, out dsMasterV, false, "1");
                if (dsMasterV.Tables[0].Rows.Count>0)
                {
                    throw new Exception("[" + dsMasterV.Tables[0].Rows[0]["ServicComponent"] + "] Servic Component and [" + dsMasterV.Tables[0].Rows[0]["DurationType"] + "] Duration Type already exists.");
                }



                string sql = "SELECT * FROM EmployeeFixedServiceMaster WHERE Id='" + SalaryHeadWiseAmountSetting.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeFixedServicMaster", out sID);
                    dr["Id"] = "SHWAS" + sID;
                    dr["SalaryHeadId"] = SalaryHeadWiseAmountSetting.SalaryHeadId;
                    dr["ServicComponent"] = SalaryHeadWiseAmountSetting.ServicComponent;
                    dr["DurationType"] = SalaryHeadWiseAmountSetting.DurationType;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    //dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["SalaryHeadId"] = SalaryHeadWiseAmountSetting.SalaryHeadId;
                    dr["ServicComponent"] = SalaryHeadWiseAmountSetting.ServicComponent;
                    dr["DurationType"] = SalaryHeadWiseAmountSetting.DurationType;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    //dr["PlantId"] = identity.PlantId;

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
                throw ex;
            }
        }

        [HttpGet,Authorize]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM EmployeeFixedServiceMaster WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class EmployeeFixedServiceMaster : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string SalaryHeadId { get; set; }
            public string ServicComponent { get; set; }
            public string DurationType { get; set; }
            public string CompanyGroupId { get; set; }
            //public string PlantId { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }
            #endregion Audit Properties
        }

        #endregion -- Operations  

    }
}