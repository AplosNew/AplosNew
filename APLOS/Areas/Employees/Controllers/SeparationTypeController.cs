using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;

namespace Aplos.Areas.Employees.Controllers
{
    /// <summary>
    /// <remark>Author:Mehedi Hasan Tamim;Date:30-12-2015;</remark>
    /// <remark>Modified:Belayet Hossain;Date:6-Jan-2016;</remark>
    /// </summary>
    public class SeparationTypeController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>
        private readonly ISeparationTypeService _separationTypeService;
        private readonly ISqlRepository _sqlRepository;
        public SeparationTypeController(ISeparationTypeService separationTypeService, ISqlRepository sqlRepository)
        {
            _separationTypeService = separationTypeService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region dll
        //dll

        /// <summary>   Creates a JSON result with the given data as its content. </summary>
        /// <returns>   The currency list. </returns>
        // [Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_separationTypeService.GetSeparationTypeList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations


        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT SalaryHeadID,SalaryHead FROM SalaryHead ORDER BY Sequence";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDataForEdit(string Id)
        {
            IEnumerable<object> SeparationTypeFixedAmount = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string SeparationTypesql = @"SELECT * FROM hkp.[SeparationType] WHERE Id='" + Id + "'";
            string SeparationTypeDetailssql = @"SELECT * FROM [SeparationTypeDetails] WHERE SeparationTypeId='" + Id + "' ORDER BY CONVERT( int ,YearNo)";
            string SeparationTypeFiexdDayAmountsql = @"SELECT EmploymentType,DayNo FROM SeparationTypeFixedDayAmount  WHERE SeparationTypeId='" + Id + "'";

            var SeparationType = _sqlRepository.GetDataCollection(SeparationTypesql);
            var SeparationTypeDetails = _sqlRepository.GetDataCollection(SeparationTypeDetailssql);
            var SeparationTypeFixedAmounttemp = _sqlRepository.GetDataCollection(SeparationTypeFiexdDayAmountsql);
            if (SeparationTypeFixedAmounttemp.Count > 0)
            {
                SeparationTypeFixedAmount = SeparationTypeFixedAmounttemp;
            }
            else
            {
                string sql = @"SELECT UserName EmploymentType,'' DayNo FROM EmploymentTypeEnum  ";
                SeparationTypeFixedAmount = _sqlRepository.GetDataCollection(sql);
            }

            return Json(new { SeparationType, SeparationTypeDetails, SeparationTypeFixedAmount }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSeparationType(SeparationType SeparationTypeData, IEnumerable<SeparationTypeDetails> SeparationTypeDetailsData, IEnumerable<SeparationTypeFixedDayAmount> SeparationTypeFixedDayAmountData)
        {
            string masterepk = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsSeparationTypeDataList;
            DataSet dsSeparationTypeDetailsDataList;
            DataSet dsSeparationTypeDetailsDataDeleteList;
            DataSet dsSeparationTypeFixedDayAmountDataDeleteList = null;
            DataSet dsSeparationTypeFixedDayAmountList = null;
            try
            {
                //if (SeparationTypeDetailsData == null )
                //{
                //    throw new Exception("Year No & Days no cannot be null..");
                //}

                //SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='' AND EmpSystemId=''
                string sql = @"SELECT * FROM [HKP].[SeparationType] WHERE PlantID = '" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsSeparationTypeDataList, false, "1");



                DataView dvSeparationTypeDataList = new DataView(dsSeparationTypeDataList.Tables[0]);
                dvSeparationTypeDataList.RowFilter = "UserName='" + SeparationTypeData.UserName.ToString() + "' AND PlantID='" + identity.PlantId + "'";

                if (dvSeparationTypeDataList.Count == 0)
                {
                    string sID = string.Empty;
                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SeparationType", out sID);

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(DateTime.Now.ToShortDateString().ToString(), "SeparationType", out sID);

                    DataRow dr = dsSeparationTypeDataList.Tables[0].NewRow();
                    dr["Id"] = "ST" + sID;
                    dr["Sequence"] = SeparationTypeData.Sequence.ToString();
                    dr["Code"] = SeparationTypeData.Code.ToString();
                    dr["ShortName"] = SeparationTypeData.ShortName.ToString();
                    dr["StandardName"] = SeparationTypeData.StandardName.ToString();
                    dr["UserName"] = SeparationTypeData.UserName.ToString();
                    if (!string.IsNullOrEmpty(SeparationTypeData.Description))
                    {
                        dr["Description"] = SeparationTypeData.Description;
                    }

                    if (!string.IsNullOrEmpty(SeparationTypeData.Remarks))
                    {
                        dr["Remarks"] = SeparationTypeData.Remarks;
                    }

                    if (!string.IsNullOrEmpty(SeparationTypeData.FormulaDes))
                    {
                        dr["FormulaDes"] = SeparationTypeData.FormulaDes.ToString();
                    }

                    if (!string.IsNullOrEmpty(SeparationTypeData.FormulaDesID))
                    {
                        dr["FormulaDesID"] = SeparationTypeData.FormulaDesID.ToString();
                    }
                    dr["IsGratuityApplicable"] = Convert.ToBoolean(SeparationTypeData.IsGratuityApplicable);
                    dr["IsFixedDayAmountApplicable"] = Convert.ToBoolean(SeparationTypeData.IsFixedDayAmountApplicable);
                    dr["IsNetPayWithFinalSattlement"] = Convert.ToBoolean(SeparationTypeData.IsNetPayWithFinalSattlement);
                    dr["PlantID"] = identity.PlantId;
                    dr["IsActive"] = Convert.ToBoolean(SeparationTypeData.IsActive);
                    dr["IsArchive"] = false;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsSeparationTypeDataList.Tables[0].Rows.Add(dr);
                    masterepk = dr["Id"].ToString();

                }
                else
                {


                    //edit
                    DataRow dr = dvSeparationTypeDataList[0].Row;

                    dr.BeginEdit();
                    dr["Sequence"] = SeparationTypeData.Sequence.ToString();
                    dr["Code"] = SeparationTypeData.Code.ToString();
                    dr["ShortName"] = SeparationTypeData.ShortName.ToString();
                    dr["StandardName"] = SeparationTypeData.StandardName.ToString();
                    dr["UserName"] = SeparationTypeData.UserName.ToString();
                    if (!string.IsNullOrEmpty(SeparationTypeData.Description))
                    {
                        dr["Description"] = SeparationTypeData.Description;
                    }

                    if (!string.IsNullOrEmpty(SeparationTypeData.Remarks))
                    {
                        dr["Remarks"] = SeparationTypeData.Remarks;
                    }

                    if (!string.IsNullOrEmpty(SeparationTypeData.FormulaDes))
                    {
                        dr["FormulaDes"] = SeparationTypeData.FormulaDes.ToString();
                    }

                    if (!string.IsNullOrEmpty(SeparationTypeData.FormulaDesID))
                    {
                        dr["FormulaDesID"] = SeparationTypeData.FormulaDesID.ToString();
                    }


                    dr["IsFixedDayAmountApplicable"] = Convert.ToBoolean(SeparationTypeData.IsFixedDayAmountApplicable);
                    dr["IsGratuityApplicable"] = Convert.ToBoolean(SeparationTypeData.IsGratuityApplicable);
                    dr["IsNetPayWithFinalSattlement"] = Convert.ToBoolean(SeparationTypeData.IsNetPayWithFinalSattlement);
                    dr["PlantID"] = identity.PlantId;
                    dr["IsActive"] = Convert.ToBoolean(SeparationTypeData.IsActive);
                    dr["IsArchive"] = false;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                    masterepk = dr["Id"].ToString();
                    dvSeparationTypeDataList.RowFilter = null;
                }



                string sqldetailsdelte = @"Delete FROM [dbo].[SeparationTypeDetails] WHERE SeparationTypeId = '" + masterepk + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqldetailsdelte, out dsSeparationTypeDetailsDataDeleteList, false, "1");


                string sqldetails = @"SELECT * FROM [dbo].[SeparationTypeDetails] WHERE SeparationTypeId = '" + masterepk + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqldetails, out dsSeparationTypeDetailsDataList, false, "1");

                int count = 0;
                if (SeparationTypeDetailsData != null)
                {
                    if (SeparationTypeDetailsData.Count() > 0)
                    {

                        DataView dvSeparationTypeDetailsDataList = new DataView(dsSeparationTypeDetailsDataList.Tables[0]);
                        dvSeparationTypeDetailsDataList.RowFilter = "SeparationTypeId='" + masterepk + "'";
                        if (dvSeparationTypeDetailsDataList.Count == 0)
                        {
                            foreach (var item in SeparationTypeDetailsData.Where(x => Convert.ToInt32(x.DayNo) > 0).ToList())
                            {
                                count++;
                                string sIDdetails = string.Empty;
                                //bplib.clsGenID objGenID = new bplib.clsGenID();
                                //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SeparationTypeDetails", out sIDdetails);
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID(DateTime.Now.ToShortDateString().ToString(), "SeparationTypeDetails", out sIDdetails);

                                DataRow dr = dsSeparationTypeDetailsDataList.Tables[0].NewRow();
                                dr["Id"] = "STD" + sIDdetails + count;
                                dr["SeparationTypeId"] = masterepk;
                                dr["DayNo"] = item.DayNo;
                                dr["YearNo"] = item.YearNo;
                                dr["RoundUp"] = item.RoundUp;
                                dr["EmploymentType"] = item.EmploymentType;
                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsSeparationTypeDetailsDataList.Tables[0].Rows.Add(dr);



                            }
                        }




                    }
                }

                if (SeparationTypeData.IsFixedDayAmountApplicable)
                {


                    string sqlFixedDayAmountdelete = @"Delete FROM [dbo].[SeparationTypeFixedDayAmount] WHERE SeparationTypeId = '" + masterepk + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sqlFixedDayAmountdelete, out dsSeparationTypeFixedDayAmountDataDeleteList, false, "1");


                    string sqlFixedDayAmount = @"SELECT * FROM [dbo].[SeparationTypeFixedDayAmount] WHERE SeparationTypeId = '" + masterepk + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sqlFixedDayAmount, out dsSeparationTypeFixedDayAmountList, false, "1");

                    if (SeparationTypeFixedDayAmountData.Count() > 0)
                    {

                        DataView dvSeparationTypeFixedDayAmountList = new DataView(dsSeparationTypeFixedDayAmountList.Tables[0]);
                        dvSeparationTypeFixedDayAmountList.RowFilter = "SeparationTypeId='" + masterepk + "'";
                        if (dvSeparationTypeFixedDayAmountList.Count == 0)
                        {
                            foreach (var item in SeparationTypeFixedDayAmountData.ToList())
                            {

                                string sIDFixeds = string.Empty;
                                bplib.clsGenID objGenID = new bplib.clsGenID();
                                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SeparationTypeFixedDayAmount", out sIDFixeds);
                                DataRow dr = dsSeparationTypeFixedDayAmountList.Tables[0].NewRow();
                                dr["Id"] = "STF" + sIDFixeds;
                                dr["SeparationTypeId"] = masterepk;
                                dr["DayNo"] = item.DayNo;
                                dr["EmploymentType"] = item.EmploymentType;
                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsSeparationTypeFixedDayAmountList.Tables[0].Rows.Add(dr);



                            }
                        }




                    }
                }

                if (SeparationTypeDetailsData != null)
                {
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsSeparationTypeDataList, dsSeparationTypeDetailsDataList, dsSeparationTypeFixedDayAmountList);
                }

                else
                {
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsSeparationTypeDataList, dsSeparationTypeFixedDayAmountList);
                }

                //    clsStaticInfo obj = new clsStaticInfo();
                //obj.SaveDataSets(dsSeparationTypeDataList, dsSeparationTypeDetailsDataList, dsSeparationTypeFixedDayAmountList);


            }
            catch (Exception ex)
            {

                throw (ex);
            }













            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // _restService.Insert(rest, identity.PlantId, restDetails);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public ActionResult GetSeparationTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id
                                , Sequence
                                , Code
                                , ShortName
                                , StandardName
                                , UserName
                                , [Description]
                                , Remarks
                                , FormulaDes	
                                , FormulaDesID	
                                , PlantID	
                                , IsGratuityApplicable
                                , IsActive      
                                 FROM [HKP].[SeparationType]
                                 WHERE PlantID='" + identity.PlantId + @"'   ORDER BY Sequence";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName  FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelistForFiexdDays()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName EmploymentType,'' DayNo FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM hkp.[SeparationType] WHERE PlantID='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSeparationType(int pageSize = 10, int pageNumber = 1, string orderBy = "asc")
        {
            var totalCount = _separationTypeService.Query().Select().Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Json(
                new
                {
                    SeparationTypeData = _separationTypeService.Query().OrderBy(r => r.OrderBy(x => x.Id)).SelectPage(pageNumber, pageSize, out totalCount),
                    count = totalCount,
                    totalPages
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSeparationTypeById(string id)
        {
            return Json(_separationTypeService.Find(id), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult Create(SeparationType SeparationType)
        {
            if (ModelState.IsValid)
            {
                _separationTypeService.Insert(SeparationType);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost, Authorize]
        public JsonResult Edit(SeparationType SeparationType)
        {
            if (ModelState.IsValid)
            {
                _separationTypeService.Update(SeparationType);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _separationTypeService.Archive(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
    public class SeparationTypeDetails
    {
        public string Id { get; set; }
        public string YearNo { get; set; }
        public string DayNo { get; set; }
        public bool RoundUp { get; set; }
        public string EmploymentType { get; set; }
    }

    public class SeparationTypeFixedDayAmount
    {
        public string Id { get; set; }
        public string DayNo { get; set; }
        public string EmploymentType { get; set; }
    }
}