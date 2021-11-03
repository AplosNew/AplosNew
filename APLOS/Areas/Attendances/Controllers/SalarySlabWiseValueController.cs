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

namespace Aplos.Areas.Attendances.Controllers
{
    public class SalarySlabWiseValueController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private ConManager objCon;

        public SalarySlabWiseValueController(
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
        public ActionResult getSalarySlabWiseValue()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select ss.Id,ss.PlantId,ss.ValueSalaryHeadId,ss.ValueSalaryHeadAmount,ss.BaseSalaryHeadId
                            ,ss.BaseSalaryHeadMinAmount,ss.BaseSalaryHeadMaxAmount,ss.Active
                            ,SH.SalaryHead+' ['+SH.HeadType+']' AS ValueSalaryHead,SHS.SalaryHead+' ['+SHS.HeadType+']' AS BaseSalaryHead
                                   From [dbo].[SalarySlabWiseValue] ss 
		                            left join SalaryHead SH ON SH.SalaryHeadID=SS.ValueSalaryHeadId 
		                            left join SalaryHead SHS ON SHS.SalaryHeadID=SS.BaseSalaryHeadId
							        where PlantId='" + identity.PlantId+ @"' 
                            order by ValueSalaryHead,BaseSalaryHead,BaseSalaryHeadMinAmount,BaseSalaryHeadMaxAmount ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Save(SalarySlabWiseValue SalarySlab)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SaveSalarySlabWiseValue(SalarySlab);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        public void SaveSalarySlabWiseValue(SalarySlabWiseValue SalarySlab)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[SalarySlabWiseValue] WHERE Id='" + SalarySlab.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string _seed = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SalarySlabWiseValue", out _seed);
                    dr["Id"] = "SSV" + _seed;
                    dr["ValueSalaryHeadId"] = SalarySlab.ValueSalaryHeadId;
                    dr["ValueSalaryHeadAmount"] = SalarySlab.ValueSalaryHeadAmount;
                    dr["BaseSalaryHeadId"] = SalarySlab.BaseSalaryHeadId;
                    dr["BaseSalaryHeadMinAmount"] = SalarySlab.BaseSalaryHeadMinAmount;
                    dr["BaseSalaryHeadMaxAmount"] = SalarySlab.BaseSalaryHeadMaxAmount;
                    dr["Active"] = SalarySlab.Active;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["ValueSalaryHeadId"] = SalarySlab.ValueSalaryHeadId;
                    dr["ValueSalaryHeadAmount"] = SalarySlab.ValueSalaryHeadAmount;
                    dr["BaseSalaryHeadId"] = SalarySlab.BaseSalaryHeadId;
                    dr["BaseSalaryHeadMinAmount"] = SalarySlab.BaseSalaryHeadMinAmount;
                    dr["BaseSalaryHeadMaxAmount"] = SalarySlab.BaseSalaryHeadMaxAmount;
                    dr["Active"] = SalarySlab.Active;
                    dr["PlantId"] = identity.PlantId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    
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

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM [dbo].[SalarySlabWiseValue] WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class SalarySlabWiseValue : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string PlantId { get; set; }
            public string ValueSalaryHeadId { get; set; }
            public decimal ValueSalaryHeadAmount { get; set; }
            public string BaseSalaryHeadId { get; set; }
            public decimal BaseSalaryHeadMinAmount { get; set; }
            public decimal BaseSalaryHeadMaxAmount { get; set; }
            public decimal Active { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
           
            #endregion Audit Properties
        }

        #endregion -- Operations  

    }
}