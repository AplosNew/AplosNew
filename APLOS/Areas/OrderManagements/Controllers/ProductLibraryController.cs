#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Library.ViewModel.Materials;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class ProductLibraryController : BaseController
    {
        #region -- Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        string DTableName = "dbo.ProductLibraryAttribute";
        public ProductLibraryController(
            IUnitOfWork U
            , ISqlRepository R
            )
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion -- Constructor

        #region Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region Operation

        [HttpGet, Authorize]
        public ActionResult GetCostingMasterTemplate()
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select qcm.*, p.UserName as Customer, pm.UserName as ProductMaster 
							,pc.UserName as ProductCategory,CUR.Code AS Currency,ct.UserName AS CostingTypeName
							,psc.UserName as ProductSubCategory
                             ,pm.CostingType
							from CostingMasterTemplate qcm 
							left join [HKP].[Party] p ON p.Id = qcm.CustomerId
                            left join scs.Currency CUR on CUR.Id=qcm.CurrencyId
                            left join [MST].[ProductMaster] pm ON pm.Id = qcm.ProductMasterId
							left join [HKP].[ProductCategory] as pc on pc.Id = pm.ProductCategoryId
							left join [HKP].[ProductSubCategory] as psc on psc.Id = pm.ProductSubCategoryId
							LEFT JOIN CostingTypes AS ct ON ct.CostingType=pm.CostingType";

                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetRecipeGlobalMasterList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT BRM.Id, BRM.CompanyGroupId, BRM.CompanyId, BRM.EntityId, BRM.ProcessId, PR.UserName AS Process,
                                BRM.Code, BRM.UserName as 'Name',BRM.Description,E.UserName Entity FROM [TRN].[RecipeGlobalMaster] AS BRM
                                LEFT JOIN [HKP].[Process] AS PR ON BRM.ProcessId=PR.Id
                                LEFT JOIN [ORG].[Entity] AS E ON BRM.EntityId=E.Id
                                Where BRM.CompanyGroupId='" + identity.CompanyGroupId + "' AND BRM.CompanyId='" + identity.CompanyId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT (ISNULL((MAX(ISNULL(Sequence,0))),0)+1) Sequence FROM [dbo].[ProductLibrary]";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductLibraryAttribute(string masterId)
        {
            
            string sql = @"SELECT PA.*,SI.UserName ScanItem,U.Code UoM FROM [dbo].[ProductLibraryAttribute] PA
                            LEFT JOIN dbo.ScanItem SI ON SI.Id=PA.ScanItemId
                            LEFT JOIN SCS.UnitOfMeasurement U ON U.Id=PA.UoMId Where PA.ProductLibraryId='" + masterId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";

            sql = @"select top 100 * from (SELECT PL.*,MM.UserName MaterialMaster, RGM.UserName Recipe, MMA.StandardName Article, PM.UserName AS ProductMasterName, CT.UserName AS CostingMasterTemplate 
                        FROM [dbo].[ProductLibrary] PL
                        LEFT JOIN MST.[MaterialMaster] MM ON MM.Id = PL.MaterialMasterId
                        LEFT JOIN [TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId
                        LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = PL.ArticleId
                        LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                        LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                        LEFT JOIN dbo.CostingMasterTemplate AS CT ON CT.Id=PL.CostingMasterTemplateId
                        WHERE PL.CompanyGroupId='" + identity.CompanyGroupId + "') AS TEMP WHERE " + strkey + " ORDER BY AddedDate DESC";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> entity , List<Dictionary<string, object>> attributes)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entity != null)
                {

                    DataRow dr;

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductLibrary WHERE Id='" + entity["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("select * from dbo.ProductLibrary where Code='" + entity["Code"] + "' AND  Id<>'" + entity["Id"] + "'", out DataSet dsCodeMaster, false, "1");
                    if (dsCodeMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Code already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from dbo.ProductLibrary where UserName='" + entity["UserName"] + "' AND  Id<>'" + entity["Id"] + "'", out DataSet dsUserMaster, false, "1");
                    if (dsUserMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same User Name already exists!!!");


                    string _Id = "";
                    string _DId = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductLibrary", out _Id);

                        entity["CompanyGroupId"] = identity.CompanyGroupId;

                        entity["AddedBy"] = identity.Name;
                        entity["AddedDate"] = System.DateTime.Now.ToString();
                        entity["AddedFromIP"] = identity.IPAddress;

                        entity["Id"] = "PL" + _Id;
                        _Id = entity["Id"].ToString();
                        AddNewRow(dsMaster.Tables[0], entity);
                    }
                    else
                    {
                        _Id = entity["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], entity);
                    }

                    #endregion data update

                    #region Child 

                    DataSet dsChild;
                    

                    con.OpenDataSetThroughAdapter("select * from " + DTableName + " where  ProductLibraryId='" + _Id + "'", out dsChild, false, "1");
                    #region data update


                    if (attributes!=null)
                    {
                        foreach (var item in attributes)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(DTableName, out _DId);

                            DataView dv = new DataView(dsChild.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            if (dv.Count == 0)
                            {
                                item["Id"] = _DId;
                                item["ProductLibraryId"] = _Id;
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


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsChild);



                }
                return Json(new { Error = false, Data = entity, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult Edit(Dictionary<string, object> entity, List<Dictionary<string, object>> attributes)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (entity != null)
                {

                    DataRow dr;

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductLibrary WHERE Id='" + entity["Id"] + "'", out dsMaster, false, "1");

                    string _Id = "";
                    string _DId = "";

                    #region data update
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductLibrary", out _Id);

                        entity["CompanyGroupId"] = identity.CompanyGroupId;

                        entity["AddedBy"] = identity.Name;
                        entity["AddedDate"] = System.DateTime.Now.ToString();
                        entity["AddedFromIP"] = identity.IPAddress;

                        entity["Id"] = "PL" + _Id;
                        _Id = entity["Id"].ToString();
                        AddNewRow(dsMaster.Tables[0], entity);
                    }
                    else
                    {
                        _Id = entity["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], entity);
                    }

                    #endregion data update

                    #region Child 

                    DataSet dsChild;


                    con.OpenDataSetThroughAdapter("select * from " + DTableName + " where  ProductLibraryId='" + _Id + "'", out dsChild, false, "1");
                    #region data update


                    if (attributes!=null)
                    {
                        foreach (var item in attributes)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(DTableName, out _DId);

                            DataView dv = new DataView(dsChild.Tables[0]);
                            dv.RowFilter = "Id='" + item["Id"] + "'";

                            if (dv.Count == 0)
                            {
                                item["Id"] = _DId;
                                item["ProductLibraryId"] = _Id;
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


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsChild);



                }
                return Json(new { Error = false, Data = entity, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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

        [HttpPost]
        public ActionResult Delete(string id)
        {
            DeleteProductLibrary(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProductLibrary(string id)
        {
            string strSQL, strDCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strDCSQL = "DELETE FROM [dbo].[ProductLibraryAttribute] Where ProductLibraryId ='" + id+"'";
                strSQL = "DELETE FROM [dbo].[ProductLibrary] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                
                objCon.ExecuteNonQueryWrapper(strDCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function


        [HttpPost,Authorize]
        public ActionResult DeleteProductLibraryAttribute(string id)
        {
            DeleteProductLibraryAttributeData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteProductLibraryAttributeData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {


                strSQL = "DELETE FROM [dbo].[ProductLibraryAttribute] Where Id ='" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        #endregion -- Operations



        [HttpPost,Authorize]
        public ActionResult ProductLibraryReport(string IDs)
        {
            try
            {
                string sql = ProductLibrarySql(IDs);
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Production Library Report";

                DataTable dtProductionLibrary = _sqlRepository.GetDataTable(sql);

                int ROW = 6;int COL = 1;

                sheet[ROW, COL].Text = "Sequence"; sheet[ROW, COL].ColumnWidth = 10;int colSequence = COL;COL++;
                sheet[ROW, COL].Text = "Code";sheet[ROW, COL].ColumnWidth = 10; int colCode = COL;COL++;
                sheet[ROW, COL].Text = "Short Name";sheet[ROW, COL].ColumnWidth = 10;int colShortName = COL;COL++;
                sheet[ROW, COL].Text = "Standard Name";int colStandardName = COL;COL++;
                sheet[ROW, COL].Text = "User Name";sheet[ROW, COL].ColumnWidth = 20;int colUserName = COL;COL++;
                sheet[ROW, COL].Text = "Material Master";sheet[ROW, COL].ColumnWidth = 30;int colMaterialMaster = COL;COL++;
                sheet[ROW, COL].Text = "Article";sheet[ROW, COL].ColumnWidth = 40;int colArticle = COL;COL++;
                sheet[ROW, COL].Text = "Display Name";sheet[ROW, COL].ColumnWidth = 16;int colDisplayName = COL;COL++;
                sheet[ROW, COL].Text = "Recipe";sheet[ROW, COL].ColumnWidth = 10;int colRecipe = COL; COL++;
                sheet[ROW, COL].Text = "Production Group";sheet[ROW, COL].ColumnWidth = 16; int colProductionGroup = COL;COL++;
                sheet[ROW, COL].Text = "Attribute";sheet[ROW, COL].ColumnWidth = 15;int colAttribute = COL;COL++;
                sheet[ROW, COL].Text = "Attribute Value";sheet[ROW, COL].ColumnWidth = 20;int colAttributeValue = COL;

                int endCol = COL;
                
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtProductionLibrary.Rows.Count; i++)
                {
                    sheet[ROW, colSequence].Text = dtProductionLibrary.Rows[i]["Sequence"].ToString();
                    sheet[ROW, colCode].Text = dtProductionLibrary.Rows[i]["Code"].ToString();
                    sheet[ROW, colShortName].Text = dtProductionLibrary.Rows[i]["ShortName"].ToString();
                    sheet[ROW, colStandardName].Text = dtProductionLibrary.Rows[i]["StandardName"].ToString();
                    sheet[ROW, colUserName].Text = dtProductionLibrary.Rows[i]["UserName"].ToString();
                    sheet[ROW, colMaterialMaster].Text = dtProductionLibrary.Rows[i]["MaterialMaster"].ToString();
                    sheet[ROW, colArticle].Text = dtProductionLibrary.Rows[i]["Article"].ToString();
                    sheet[ROW, colDisplayName].Text = dtProductionLibrary.Rows[i]["RecipeOrProductionGroup"].ToString();
                    sheet[ROW, colRecipe].Text = dtProductionLibrary.Rows[i]["Recipe"].ToString();
                    sheet[ROW, colProductionGroup].Text = dtProductionLibrary.Rows[i]["ProductionGroup"].ToString();
                    sheet[ROW, colAttribute].Text = dtProductionLibrary.Rows[i]["Attribute"].ToString();
                    sheet[ROW, colAttributeValue].Text = dtProductionLibrary.Rows[i]["AttributeValue"].ToString();
                 
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }
            
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.AutoFilters.FilterRange = sheet.Range[StartRow - 1, 1, ROW, endCol];

                sheet["A" + StartRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Product Library", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Product Library Report.xlsx";
                

                workbook.Version = ExcelVersion.Excel2013;
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
        }
        private string ProductLibrarySql(string IDs)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT PL.*,MM.UserName MaterialMaster, RGM.UserName Recipe, MMA.StandardName Article, 
PM.UserName AS ProductMasterName,PLA.UserName Attribute,PLA.AttributeValue
FROM [dbo].[ProductLibrary] PL
LEFT JOIN MST.[MaterialMaster] MM ON MM.Id = PL.MaterialMasterId
LEFT JOIN [TRN].[RecipeGlobalMaster] RGM ON RGM.Id = PL.RecipeId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = PL.ArticleId
LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
LEFT JOIN [dbo].[ProductLibraryAttribute] PLA ON PLA.ProductLibraryId=PL.Id
WHERE PL.CompanyGroupId='" + identity.CompanyGroupId + @"' --and ISnull( PL.Id,'') in("+IDs+@")
ORDER BY PL.Sequence  ";

        }

    }
}