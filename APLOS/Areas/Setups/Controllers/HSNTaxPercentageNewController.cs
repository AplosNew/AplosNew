using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Taxations;
using Library.Service.Taxations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Aplos.Areas.Setups.Controllers
{
    public class HSNTaxPercentageNewController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        private readonly IHSNTaxPercentageService _hSNTaxPercentageService;

        public HSNTaxPercentageNewController(IHSNTaxPercentageService hSNTaxPercentageService, IUnitOfWork U, ISqlRepository R)
        {
            _hSNTaxPercentageService = hSNTaxPercentageService;
            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor


        #region Actions
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string countryId)
        {
            return Json(_hSNTaxPercentageService.GetList(parameters, countryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Save(List<Dictionary<string, string>> data, string taxtype)
        {
            try
            {
                List<Dictionary<string, string>> tempList = new List<Dictionary<string, string>>();
                if (taxtype.ToUpper() == "NORMAL TAX")
                {

                    tempList = data.Where(ee => ee["TYPEID"] == "HSN").ToList();
                    if (tempList != null && tempList.Count > 0)
                        saveTaxData(tempList);
                }
                else
                {
                    string sql = "SELECT * FROM HKP.SpecialTax where id='" + taxtype + "'";
                    DataTable _specialTax = _sqlRepository.GetDataTable(sql);

                    tempList = data.Where(ee => ee["TYPEID"] == "SPECIALTAX").ToList();
                    if (tempList != null && tempList.Count > 0)
                    {
                        if (bplib.clsWebLib.GetBoolData(_specialTax.Rows[0]["IsSpacifyToHSNCode"].ToString()) == false)
                            SpecialTaxData(tempList);
                        else
                            SpecialTaxDataForAllHSN(tempList, taxtype);
                    }

                }
                return Json(new { Message = "Data saved successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult ExcelExport(List<Dictionary<string, string>> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data found");

                if (data.Count == 0)
                    throw new Exception("No data found");


                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }


                GridToExcelReport(dt);


                return null;
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }

        [HttpGet]
        public ActionResult Download()
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                string fullPath = Path.Combine(HostingEnvironment.MapPath("~") + "autodata.xlsx");
                IWorkbook workbook1 = excelEngine.Excel.Workbooks.Open(fullPath);
                System.IO.File.Delete(fullPath);

                return RenderReportAsExcel(workbook1, "autodata.xlsx");
            }
            catch (Exception)
            {


            }
            return View();
        }

        private void GridToExcelReport(DataTable data)
        {
            try
            { //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~") + "autodata.xlsx");

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    sheet.ImportDataTable(data, true, 2, 1);
                    //sheet.ImportData(data.Select(), 1, 1, true);
                    workbook.SaveAs(fullPath);
                    //workbook.SaveAs("autodata.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
                    // return View();
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }


        }

        [HttpPost]
        public JsonResult GetTaxPercentage(string countryid, string[] taxcodes, string effectivedate, string SpecialTaxID)
        {


            try
            {

                TransposeData(countryid, taxcodes, effectivedate, out List<Dictionary<string, string>> FINAL, out List<string> columnNames, SpecialTaxID);

                return Json(new { DATA = FINAL, COLUMNS = columnNames }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult GetTaxCategories(string countryid)
        {


            try
            {
                string sql = "select * from [MST].[TaxCategory] where countryid='" + countryid + "' order by sequence";


                return Json(_sqlRepository.GetModelCollection<TaxCategory>(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetTaxForAddNew(GridParameter parameters, string countryId)
        {
            return Json(_hSNTaxPercentageService.GetList(parameters, countryId), JsonRequestBehavior.AllowGet);



        }


        [HttpGet]
        public ActionResult GetHNSList(GridParameter parameters, string countryId)
        {
            return Json(_hSNTaxPercentageService.GetHSNList(parameters, countryId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetSpecialTaxList(GridParameter parameters, string countryId)
        {
            string sql = @"SELECT 'Normal Tax' AS Id, 'Normal Tax' AS SpecialTax
                            UNION ALL
                            SELECT Id,Code AS SpecialTax FROM HKP.SpecialTax";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<HSNTaxPercentage> hSNTaxPercentage, string countryId)
        {
            _hSNTaxPercentageService.InsertOrUpdate(hSNTaxPercentage, countryId);
            return Json(new { HSNTaxPercentage = hSNTaxPercentage, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(HSNTaxPercentage hSNTaxPercentage)
        {
            _hSNTaxPercentageService.Update(hSNTaxPercentage);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _hSNTaxPercentageService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Actions

        #region  Custom Fucntions

        private void TransposeData(string countryid, string[] taxcodes, string effectivedate, out List<Dictionary<string, string>> FINAL, out List<string> columnNames, string SpecialTaxID)
        {
            try
            {
                //string SpecialTax = "Normal Tax";

                string taxes = "''";
                for (int i = 0; i < taxcodes.Length; i++)
                    taxes += ",'" + taxcodes[i] + "'";


                string sql = "select * from [MST].[TaxCategory] where id in (" + taxes + ") order by sequence";
                DataTable _taxData = _sqlRepository.GetDataTable(sql);


                sql = "SELECT * FROM HKP.SpecialTax where id='" + SpecialTaxID + "'";
                DataTable _specialTax = _sqlRepository.GetDataTable(sql);



                //"Normal Tax" 
                if (effectivedate == "")
                {
                    //FOR EDIT
                    if (_specialTax.Rows.Count == 0)
                    {
                        sql = @"
                            select * from (
                            SELECT tp.Id AS SavedSystemID,h.Id HSNCodeID,ch.CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            tp.Percentage,'HSN' AS TYPEID
                              FROM hkp.HSNCode AS h
                            INNER JOIN scs.CountryHSNCode AS ch ON ch.HSNCodeId=h.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId=ch.CountryId
                            INNER JOIN [MST].[HSNTaxPercentage] TP ON tp.CountryId=ch.CountryId AND tp.HSNCodeId=h.Id AND tp.TaxCategoryId=tax.Id

                            WHERE ch.CountryId='" + countryid + "' AND tax.id IN (" + taxes + @")


                            --union ALL
                            --
                            --SELECT  tp.Id AS SavedSystemID,h.Id HSNCodeID,'" + countryid + @"' AS CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            --tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            --CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            --tp.Percentage,'SPECIALTAX' AS TYPEID
                            --FROM hkp.SpecialTax AS h
                            --LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId='" + countryid + @"'
                            --INNER JOIN [MST].[HSNTaxPercentage] TP ON tp.TaxCategoryId=tax.Id AND tp.SpecialTaxId=h.Id AND TP.CountryId='" + countryid + @"' 
                              

                                --WHERE tax.id IN (" + taxes + @")
                            ) AS K

                            ORDER BY k.TYPEID,K.EffectiveDate DESC, k.HSNCode,K.TaxCategoryCode";

                    }
                    else
                    {
                        if (bplib.clsWebLib.GetBoolData(_specialTax.Rows[0]["IsSpacifyToHSNCode"].ToString()) == false)
                        {

                            sql = @"
                            select * from (
                           
                            SELECT  tp.Id AS SavedSystemID,h.Id HSNCodeID,'" + countryid + @"' AS CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            tp.Percentage,'SPECIALTAX' AS TYPEID
                            FROM hkp.SpecialTax AS h
                            LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId='" + countryid + @"'
                            INNER JOIN [MST].[HSNTaxPercentage] TP ON tp.TaxCategoryId=tax.Id AND tp.SpecialTaxId=h.Id AND TP.CountryId='" + countryid + @"' 
                              

                                WHERE h.id='" + SpecialTaxID + "' AND tax.id IN (" + taxes + @")
                            ) AS K

                            ORDER BY k.TYPEID,K.EffectiveDate DESC, k.HSNCode,K.TaxCategoryCode";


                        }
                        else
                        {
                            sql = @"
                            select * from (
                            SELECT tp.Id AS SavedSystemID,h.Id HSNCodeID,ch.CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            tp.Percentage,'SPECIALTAX' AS TYPEID
                              FROM hkp.HSNCode AS h
                            INNER JOIN scs.CountryHSNCode AS ch ON ch.HSNCodeId=h.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId=ch.CountryId
                            INNER JOIN [MST].[HSNTaxPercentage] TP ON tp.CountryId=ch.CountryId AND tp.HSNCodeId=h.Id AND tp.TaxCategoryId=tax.Id

                            WHERE ch.CountryId='" + countryid + "' AND tax.id IN (" + taxes + @")
                           AND  TP.SpecialTaxID='" + SpecialTaxID + @"'
                        
                            ) AS K

                            ORDER BY k.TYPEID,K.EffectiveDate DESC, k.HSNCode,K.TaxCategoryCode";

                        }


                    }
                }
                else
                {
                    if (_specialTax.Rows.Count == 0)
                    {
                        //FOR ADDNEW
                        sql = @"

                        select * from (SELECT tp.Id AS SavedSystemID,h.Id HSNCodeID,ch.CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            tp.Percentage,'HSN' AS TYPEID
                              FROM hkp.HSNCode AS h
                            INNER JOIN scs.CountryHSNCode AS ch ON ch.HSNCodeId=h.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId=ch.CountryId
                            LEFT OUTER JOIN [MST].[HSNTaxPercentage] TP ON  tp.EffectiveDate='" + effectivedate + @"'
                            and tp.CountryId=ch.CountryId AND tp.HSNCodeId=h.Id AND tp.TaxCategoryId=tax.Id-- AND 1=2--this line will control addnew/edit

                            WHERE ch.CountryId='" + countryid + "' AND tax.id IN (" + taxes + @")

                          --  union ALL
                          --
                          -- SELECT  tp.Id AS SavedSystemID,h.Id HSNCodeID,'" + countryid + @"' AS CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                          -- tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                          -- CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                          -- tp.Percentage,'SPECIALTAX' AS TYPEID
                          -- FROM hkp.SpecialTax AS h
                          -- LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId='" + countryid + @"'
                          -- LEFT OUTER JOIN [MST].[HSNTaxPercentage] TP ON tp.TaxCategoryId=tax.Id AND tp.SpecialTaxId=h.Id AND TP.CountryId='" + countryid + @"' 
                          --     AND TP.EffectiveDate='" + effectivedate + @"'
                          --
                          --     WHERE tax.id IN (" + taxes + @")

                        ) AS K

                           ORDER BY  k.TYPEID,K.EffectiveDate DESC, k.HSNCode,K.TaxCategoryCode";
                    }
                    else
                    {
                        if (bplib.clsWebLib.GetBoolData(_specialTax.Rows[0]["IsSpacifyToHSNCode"].ToString()) == false)
                        {
                            sql = @"

                        select * from (
                            
                            SELECT  tp.Id AS SavedSystemID,h.Id HSNCodeID,'" + countryid + @"' AS CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            tp.Percentage,'SPECIALTAX' AS TYPEID
                            FROM hkp.SpecialTax AS h
                            LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId='" + countryid + @"'
                            LEFT OUTER JOIN [MST].[HSNTaxPercentage] TP ON tp.TaxCategoryId=tax.Id AND tp.SpecialTaxId=h.Id AND TP.CountryId='" + countryid + @"' 
                                AND TP.EffectiveDate='" + effectivedate + @"'
                            
                                WHERE h.id='" + SpecialTaxID + "' AND tax.id IN (" + taxes + @")

                            ) AS K

                           ORDER BY  k.TYPEID,K.EffectiveDate DESC, k.HSNCode,K.TaxCategoryCode";
                        }
                        else
                        {
                            //FOR ADDNEW
                            sql = @"

                        select * from (SELECT tp.Id AS SavedSystemID,h.Id HSNCodeID,ch.CountryId, h.Code AS HSNCode, h.[Description] AS HSNDesc,
                            tax.Id AS TaxCategoryID,tax.Code AS TaxCategoryCode,tax.UserName AS TaxCategoryName,
                            CONVERT(VARCHAR,(FORMAT(isnull(TP.EffectiveDate,'" + effectivedate + @"'),'dd-MMM-yyyy'))) AS EffectiveDate,
                            tp.Percentage,'SPECIALTAX' AS TYPEID
                              FROM hkp.HSNCode AS h
                            INNER JOIN scs.CountryHSNCode AS ch ON ch.HSNCodeId=h.Id
                            LEFT OUTER JOIN [MST].[TaxCategory] tax ON tax.CountryId=ch.CountryId
                            LEFT OUTER JOIN [MST].[HSNTaxPercentage] TP ON  tp.EffectiveDate='" + effectivedate + @"' AND  TP.SpecialTaxID='" + SpecialTaxID + @"'
                            and tp.CountryId=ch.CountryId AND tp.HSNCodeId=h.Id AND tp.TaxCategoryId=tax.Id-- AND 1=2--this line will control addnew/edit

                            WHERE ch.CountryId='" + countryid + "' AND tax.id IN (" + taxes + @")
                            
                        ) AS K

                           ORDER BY  k.TYPEID,K.EffectiveDate DESC, k.HSNCode,K.TaxCategoryCode";

                        }


                    }
                }


                DataTable _data = _sqlRepository.GetDataTable(sql);


                List<string> taxlist = new List<string>();
                for (int i = 0; i < _taxData.Rows.Count; i++)
                {
                    taxlist.Add(_taxData.Rows[i]["Code"].ToString());
                }


                Dictionary<string, string> ROW = new Dictionary<string, string>();
                FINAL = new List<Dictionary<string, string>>();


                //transform the data into row column level
                columnNames = new List<string>();
                columnNames.Add("PK");
                columnNames.Add("CountryID");
                columnNames.Add("HSNCode");
                columnNames.Add("TYPEID");
                columnNames.Add("HSNCodeID");
                columnNames.Add("EffectiveDate");
                string current = "";
                StringCollection strCol = new StringCollection();
                for (int i = 0; i < _data.Rows.Count; i++)
                {
                    if (strCol.Contains(_data.Rows[i]["HSNCode"].ToString() + _data.Rows[i]["EffectiveDate"].ToString()) == true)
                        continue;



                    strCol.Add(_data.Rows[i]["HSNCode"].ToString() + _data.Rows[i]["EffectiveDate"].ToString());

                    ROW = new Dictionary<string, string>();

                    ROW.Add("PK", i.ToString() + "-" + _data.Rows[i]["HSNCode"].ToString());
                    ROW.Add("CountryID", _data.Rows[i]["CountryId"].ToString());
                    ROW.Add("HSNCode", _data.Rows[i]["HSNCode"].ToString());
                    ROW.Add("TYPEID", _data.Rows[i]["TYPEID"].ToString());
                    ROW.Add("HSNCodeID", _data.Rows[i]["HSNCodeID"].ToString());
                    ROW.Add("EffectiveDate", _data.Rows[i]["EffectiveDate"].ToString());

                    _data.DefaultView.RowFilter = "HSNCode='" + _data.Rows[i]["HSNCode"].ToString() + "' AND EffectiveDate=#" + _data.Rows[i]["EffectiveDate"].ToString() + "#";
                    DataView dv = new DataView(_data.DefaultView.ToTable());
                    foreach (string s in taxlist)
                    {
                        dv.RowFilter = "TaxCategoryCode='" + s + "'";
                        string savedSystemID = "";
                        string percentage = "";
                        if (dv.Count > 0)
                        {
                            savedSystemID = dv[0]["SavedSystemID"].ToString();
                            percentage = dv[0]["Percentage"].ToString();
                        }

                        ROW.Add(s + "SavedSystemID", savedSystemID);
                        ROW.Add(s, percentage);
                        if (columnNames.Contains(s) == false)
                        {
                            columnNames.Add(s);
                            columnNames.Add(s + "SavedSystemID");
                        }
                    }

                    FINAL.Add(ROW);

                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }
        private void saveTaxData(List<Dictionary<string, string>> data)
        {


            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef;
            try
            {
                if (data == null || data.Count == 0)
                    throw new Exception("No data found");

                string sql = "select * from [MST].[TaxCategory] where countryID='" + data[0]["CountryID"] + "' order by sequence";
                DataTable _taxData = _sqlRepository.GetDataTable(sql);


                string effectivedates = "''";
                StringCollection strCol = new StringCollection();
                for (int i = 0; i < data.Count; i++)
                {
                    if (strCol.Contains(data[i]["EffectiveDate"]) == false)
                    {
                        strCol.Add(data[i]["EffectiveDate"]);

                        effectivedates += ",'" + data[i]["EffectiveDate"] + "'";
                    }
                }

                string strSql = @"";

                if (data.Count == 1)
                    strSql = @"SELECT * FROM [MST].[HSNTaxPercentage] WHERE CountryId='" + data[0]["CountryID"] + "' and HSNCodeID='" + data[0]["HSNCodeID"] + "' AND EffectiveDate IN (" + effectivedates + ")";
                else
                    strSql = @"SELECT * FROM [MST].[HSNTaxPercentage] WHERE CountryId='" + data[0]["CountryID"] + "' AND EffectiveDate IN (" + effectivedates + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string id = "";
                bplib.clsGenID objID = new bplib.clsGenID();
                objID.GenID("TAX PERCENTAGE", out id);

                for (int ROW = 0; ROW < data.Count; ROW++)
                {
                    if (data[ROW].ContainsKey("PK") == false)
                        continue;


                    int subIndex = 0;
                    foreach (var item in data[ROW].Keys)
                    {
                        _taxData.DefaultView.RowFilter = "code='" + item + "'";
                        if (_taxData.DefaultView.Count > 0)
                        {
                            dsRef.Tables[0].DefaultView.RowFilter = "HSNCodeId='" + data[ROW]["HSNCodeID"] + "' and TaxCategoryId='" + _taxData.DefaultView[0]["ID"].ToString() + "' and EffectiveDate=#" + data[ROW]["EffectiveDate"] + "#";//old systemid
                            if (Convert.ToDouble(bplib.clsWebLib.GetNumData(data[ROW][item])) > 0)
                            {
                                if (dsRef.Tables[0].DefaultView.Count == 0)
                                {
                                    subIndex++;
                                    //addnew
                                    DataRow dr = dsRef.Tables[0].NewRow();
                                    dr["id"] = id + (ROW + 1) + subIndex;
                                    dr["CountryId"] = data[ROW]["CountryID"];
                                    dr["HSNCodeId"] = data[ROW]["HSNCodeID"];
                                    dr["TaxCategoryId"] = _taxData.DefaultView[0]["id"].ToString();
                                    dr["EffectiveDate"] = data[ROW]["EffectiveDate"];
                                    dr["Percentage"] = bplib.clsWebLib.GetNumData(data[ROW][item]);

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = System.DateTime.Now.ToString();
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;

                                    dsRef.Tables[0].Rows.Add(dr);


                                }
                                else
                                {
                                    //edit
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    dr["CountryId"] = data[ROW]["CountryID"];
                                    dr["HSNCodeId"] = data[ROW]["HSNCodeID"];
                                    dr["TaxCategoryId"] = _taxData.DefaultView[0]["id"].ToString();
                                    dr["EffectiveDate"] = data[ROW]["EffectiveDate"];
                                    dr["Percentage"] = bplib.clsWebLib.GetNumData(data[ROW][item]);

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;

                                    dr.EndEdit();

                                }
                            }
                            else
                            {
                                //delete block
                                while (dsRef.Tables[0].DefaultView.Count > 0)
                                    dsRef.Tables[0].DefaultView[0].Delete();
                            }





                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsRef);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        private void SpecialTaxData(List<Dictionary<string, string>> data)
        {


            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef;
            try
            {
                if (data == null || data.Count == 0)
                    throw new Exception("No data found");

                string sql = "select * from [MST].[TaxCategory] where countryID='" + data[0]["CountryID"] + "' order by sequence";
                DataTable _taxData = _sqlRepository.GetDataTable(sql);



                //string specialTaxID = "";
                //if (_specialTax.Rows.Count > 0)
                //    if (bplib.clsWebLib.GetBoolData(_specialTax.Rows[0]["IsSpacifyToHSNCode"].ToString()) == true)
                //        specialTaxID = taxtype;



                string effectivedates = "''";
                StringCollection strCol = new StringCollection();
                for (int i = 0; i < data.Count; i++)
                {
                    if (strCol.Contains(data[i]["EffectiveDate"]) == false)
                    {
                        strCol.Add(data[i]["EffectiveDate"]);

                        effectivedates += ",'" + data[i]["EffectiveDate"] + "'";
                    }
                }

                string strSql = @"";

                if (data.Count == 1)
                    strSql = @"SELECT * FROM [MST].[HSNTaxPercentage] WHERE CountryId='" + data[0]["CountryID"] + "' and SpecialTaxId='" + data[0]["HSNCodeID"] + "' AND EffectiveDate IN (" + effectivedates + ")";
                else
                    strSql = @"SELECT * FROM [MST].[HSNTaxPercentage] WHERE CountryId='" + data[0]["CountryID"] + "' AND EffectiveDate IN (" + effectivedates + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string id = "";
                bplib.clsGenID objID = new bplib.clsGenID();
                objID.GenID("TAX PERCENTAGE", out id);

                for (int ROW = 0; ROW < data.Count; ROW++)
                {
                    if (data[ROW].ContainsKey("PK") == false)
                        continue;


                    int subIndex = 0;
                    foreach (var item in data[ROW].Keys)
                    {
                        _taxData.DefaultView.RowFilter = "code='" + item + "'";
                        if (_taxData.DefaultView.Count > 0)
                        {
                            dsRef.Tables[0].DefaultView.RowFilter = "SpecialTaxId='" + data[ROW]["HSNCodeID"] + "' and TaxCategoryId='" + _taxData.DefaultView[0]["ID"].ToString() + "' and EffectiveDate=#" + data[ROW]["EffectiveDate"] + "#";//old systemid
                            if (Convert.ToDouble(bplib.clsWebLib.GetNumData(data[ROW][item])) > 0)
                            {
                                if (dsRef.Tables[0].DefaultView.Count == 0)
                                {
                                    subIndex++;
                                    //addnew
                                    DataRow dr = dsRef.Tables[0].NewRow();
                                    dr["id"] = id + (ROW + 1) + subIndex;
                                    dr["CountryId"] = data[ROW]["CountryID"];


                                    dr["SpecialTaxId"] = data[ROW]["HSNCodeID"];



                                    dr["TaxCategoryId"] = _taxData.DefaultView[0]["id"].ToString();
                                    dr["EffectiveDate"] = data[ROW]["EffectiveDate"];
                                    dr["Percentage"] = bplib.clsWebLib.GetNumData(data[ROW][item]);

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = System.DateTime.Now.ToString();
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;

                                    dsRef.Tables[0].Rows.Add(dr);


                                }
                                else
                                {
                                    //edit
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    dr["CountryId"] = data[ROW]["CountryID"];
                                    dr["SpecialTaxId"] = data[ROW]["HSNCodeID"];
                                    dr["TaxCategoryId"] = _taxData.DefaultView[0]["id"].ToString();
                                    dr["EffectiveDate"] = data[ROW]["EffectiveDate"];
                                    dr["Percentage"] = bplib.clsWebLib.GetNumData(data[ROW][item]);

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;

                                    dr.EndEdit();

                                }
                            }
                            else
                            {
                                //delete block
                                while (dsRef.Tables[0].DefaultView.Count > 0)
                                    dsRef.Tables[0].DefaultView[0].Delete();
                            }





                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsRef);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        private void SpecialTaxDataForAllHSN(List<Dictionary<string, string>> data, string taxtype)
        {


            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef;
            try
            {
                if (data == null || data.Count == 0)
                    throw new Exception("No data found");

                string sql = "select * from [MST].[TaxCategory] where countryID='" + data[0]["CountryID"] + "' order by sequence";
                DataTable _taxData = _sqlRepository.GetDataTable(sql);


                //string specialTaxID = "";
                //if (_specialTax.Rows.Count > 0)
                //    if (bplib.clsWebLib.GetBoolData(_specialTax.Rows[0]["IsSpacifyToHSNCode"].ToString()) == true)
                //        specialTaxID = taxtype;



                string effectivedates = "''";
                StringCollection strCol = new StringCollection();
                for (int i = 0; i < data.Count; i++)
                {
                    if (strCol.Contains(data[i]["EffectiveDate"]) == false)
                    {
                        strCol.Add(data[i]["EffectiveDate"]);

                        effectivedates += ",'" + data[i]["EffectiveDate"] + "'";
                    }
                }

                string strSql = @"";

                if (data.Count == 1)
                {
                    strSql = @"SELECT * FROM [MST].[HSNTaxPercentage] WHERE CountryId='" + data[0]["CountryID"] + "' AND HSNCodeID='" + data[0]["HSNCodeID"] + "' and SpecialTaxId='" + taxtype + "' AND EffectiveDate IN (" + effectivedates + ")";

                }
                else
                    strSql = @"SELECT * FROM [MST].[HSNTaxPercentage] WHERE CountryId='" + data[0]["CountryID"] + "' AND EffectiveDate IN (" + effectivedates + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string id = "";
                bplib.clsGenID objID = new bplib.clsGenID();
                objID.GenID("TAX PERCENTAGE", out id);

                for (int ROW = 0; ROW < data.Count; ROW++)
                {
                    if (data[ROW].ContainsKey("PK") == false)
                        continue;


                    int subIndex = 0;
                    foreach (var item in data[ROW].Keys)
                    {
                        _taxData.DefaultView.RowFilter = "code='" + item + "'";
                        if (_taxData.DefaultView.Count > 0)
                        {
                            dsRef.Tables[0].DefaultView.RowFilter = "HSNCodeID='" + data[ROW]["HSNCodeID"] + "' and SpecialTaxId='" + taxtype + "' and TaxCategoryId='" + _taxData.DefaultView[0]["ID"].ToString() + "' and EffectiveDate=#" + data[ROW]["EffectiveDate"] + "#";//old systemid
                            if (Convert.ToDouble(bplib.clsWebLib.GetNumData(data[ROW][item])) > 0)
                            {
                                if (dsRef.Tables[0].DefaultView.Count == 0)
                                {
                                    subIndex++;
                                    //addnew
                                    DataRow dr = dsRef.Tables[0].NewRow();
                                    dr["id"] = id + (ROW + 1) + subIndex;
                                    dr["CountryId"] = data[ROW]["CountryID"];


                                    dr["SpecialTaxId"] = taxtype;
                                    dr["HSNCodeId"] = data[ROW]["HSNCodeID"];



                                    dr["TaxCategoryId"] = _taxData.DefaultView[0]["id"].ToString();
                                    dr["EffectiveDate"] = data[ROW]["EffectiveDate"];
                                    dr["Percentage"] = bplib.clsWebLib.GetNumData(data[ROW][item]);

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = System.DateTime.Now.ToString();
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;

                                    dsRef.Tables[0].Rows.Add(dr);


                                }
                                else
                                {
                                    //edit
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    dr["CountryId"] = data[ROW]["CountryID"];




                                    dr["SpecialTaxId"] = taxtype;
                                    dr["HSNCodeId"] = data[ROW]["HSNCodeID"];



                                    dr["TaxCategoryId"] = _taxData.DefaultView[0]["id"].ToString();
                                    dr["EffectiveDate"] = data[ROW]["EffectiveDate"];
                                    dr["Percentage"] = bplib.clsWebLib.GetNumData(data[ROW][item]);

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;

                                    dr.EndEdit();

                                }
                            }
                            else
                            {
                                //delete block
                                while (dsRef.Tables[0].DefaultView.Count > 0)
                                    dsRef.Tables[0].DefaultView[0].Delete();
                            }





                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsRef);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        #endregion Custom Functions


    }

    public class HSNCodeData : BaseModel
    {
        public string SavedSystemID { get; set; } = "";
        public string HSNCodeID { get; set; } = "";
        public string CountryId { get; set; } = "";
        public string HSNCode { get; set; } = "";
        public string HSNDesc { get; set; } = "";
        public string TaxCategoryID { get; set; } = "";
        public string TaxCategoryCode { get; set; } = "";
        public string TaxCategoryName { get; set; } = "";
        public string EffectiveDate { get; set; } = "";
        public double Percentage { get; set; } = 0;
    }

}