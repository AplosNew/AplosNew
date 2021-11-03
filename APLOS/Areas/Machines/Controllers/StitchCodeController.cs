#region using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Machines;
using Library.Model.Machines;
using System;
using System.Data;
using System.Collections.Generic;
using Library.Security.Core;
using Library.Data.Sql;
using Library.Service.Systems;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class StitchCodeController : BaseController
    {
        #region Constructor
        private readonly IStitchCodeService _stitchCodeService;
        private readonly ISqlRepository _sqlRepository;

        public StitchCodeController(IStitchCodeService stitchCodeService, ISqlRepository R)
        {
            _stitchCodeService = stitchCodeService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
     
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_stitchCodeService.GetCbo(idntity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_stitchCodeService.Query(parameters, idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_stitchCodeService.GetAutoSequence(idntity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(StitchCode entity)
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = idntity.CompanyGroupId;
            if (string.IsNullOrEmpty(entity.PlantId))
                entity.PlantId = idntity.PlantId;
            _stitchCodeService.Insert(entity);
            return Json(new { entity, Sequence = _stitchCodeService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(StitchCode entity)
        {
            _stitchCodeService.Update(entity);
            return Json(new { Sequence = _stitchCodeService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            var entity = _stitchCodeService.Find(id);
            _stitchCodeService.Delete(entity);
            return Json(new { Sequence = _stitchCodeService.GetAutoSequence(entity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        [HttpPost, Authorize]
        public JsonResult CreateSPI(List<SPIFormula> data)
        {
            try
            {
                SaveSPIData(data);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        //private string GetPK()
        //{
        //    return GetAutoNumber(nameof(SPIFormula), PKGeneratorEnum.Auto, null, DateTime.Now);
        //}

        public void SaveSPIData(List<SPIFormula> data)
        {
            try
            {
                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    string _Id = "";
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SPIFormula", out _Id);
                    int count = 0;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[SPIFormula] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            count++;
                        
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = "S" + _Id + "-" + count;
                            dr["SPI"] = item.SPI;
                            dr["StitchCodeId"] = item.StitchCodeId;
                            dr["IsFormula"] = item.IsFormula;
                            dr["FixedValue"] = item.FixedValue;
                            dr["Formula"] = item.Formula;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["SPI"] = item.SPI;
                            dr["StitchCodeId"] = item.StitchCodeId;
                            dr["IsFormula"] = item.IsFormula;
                            dr["FixedValue"] = item.FixedValue;
                            dr["Formula"] = item.Formula;

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

                throw ex;
            }

        }

        [HttpGet, Authorize]
        public JsonResult GetSPIFormulaList(string StitchCodeId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from dbo.SPIFormula Where StitchCodeId='"+ StitchCodeId + "' ORDER BY SPI";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
    public class SPIFormula
    {
        public string Id { get; set; }
        public int SPI { get; set; }
        public string StitchCodeId { get; set; }
        public bool IsFormula { get; set; }
        public decimal FixedValue { get; set; }
        public string Formula { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }
}