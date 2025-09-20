#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MachineBudgetController : BaseController
    {
        #region -- Constrator
        private readonly IMaterialMasterMachineProcessService _baseService;
        private readonly IMaterialMasterArticleService _articleService;
        private readonly ISqlRepository _sqlRepository;
        public MachineBudgetController(IMaterialMasterMachineProcessService baseService, IMaterialMasterArticleService articleService, ISqlRepository R)
        {
            _baseService = baseService;
            _articleService = articleService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult MachineTransfer()
        {
            return View();
        }
        #endregion

        #region -- Machines
        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_baseService.GetMaterialMasterList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetDetailList(string materialMasterId)
        {
            return Json(_baseService.GetDetailList(materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetArticleList(string materialMasterId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT MMA.Id, MMA.MaterialMasterId, MMA.Code, MMA.ShortName, MMA.StandardName, MMA.RPM, MMA.MachineAllowance, MMA.StitchCodeId,SC.UserName StitchCode
		                    FROM MST.MaterialMasterArticle MMA
							LEFT JOIN HKP.StitchCode SC ON SC.Id=MMA.StitchCodeId WHERE MaterialMasterId='" + materialMasterId + "'";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region Machine Transfer
        [HttpPost, Authorize]
        public ActionResult GetMachineBudgetByFromEntity(string EntityId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT MB.*,MM.UserName Material,P.UserName Plant,ISNULL(E.UserName,'ALL') Entity,MMA.StandardName Article 
                            FROM [dbo].[MachineBudget] MB
                            LEFT JOIN ORG.Plant P ON P.Id=MB.PlantId
                            LEFT JOIN ORG.Entity E ON E.Id=MB.EntityId
                            LEFT JOIN ORG.Company C ON C.Id=P.CompanyId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MB.ArticleId
                            LEFT JOIN MST.MaterialMaster  MM ON MM.Id=MMA.MaterialMasterId
                            Where MB.EntityId='" + EntityId + "'";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        //#region  ---Budget  

        [HttpGet, Authorize]
        public ActionResult GetProductionEntityCbo(string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.General.Organization.OrganizationAuthorization orgAuth = new Library.General.Organization.OrganizationAuthorization();
                return Json(orgAuth.GetEntityByUser(plantId, identity.UserId, identity.IsSysAdmin), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private bool CheckCombination(Dictionary<string, object> data)
        {
            try
            {

                var _sql = @"SELECT * FROM [dbo].[MachineBudget] where id<>'" + data["Id"] + "' and ArticleId='" + data["ArticleId"] + "' AND PlantId='" + data["PlantId"] + "' AND ISNULL(EntityId,'ALL')='" + data["EntityId"] + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool CheckEntityCombination(Dictionary<string, object> data)
        {
            try
            {
                string _sql, sql = "";
                if (data["EntityId"].ToString().ToUpper() == "ALL")
                    _sql = @"SELECT * FROM [dbo].[MachineBudget] where  ArticleId='" + data["ArticleId"] + "' AND PlantId='" + data["PlantId"] + @"' 
                    AND ( id<>'" + data["Id"] + "' and ISNULL(EntityId,'')='' or   ISNULL(EntityId,'')<>'' ) ";

                //else
                //    _sql = @"SELECT * FROM [dbo].[MachineBudget] where ArticleId='" + data["ArticleId"] + "' AND PlantId='" + data["PlantId"] + @"' 
                //            AND ( id<>'" + data["Id"] + "' and ISNULL(EntityId,'')<>'' or ISNULL(EntityId,'')='' )";
                else
                    _sql = @"SELECT * FROM [dbo].[MachineBudget] where ArticleId='" + data["ArticleId"] + "' AND PlantId='" + data["PlantId"] + @"' 
                            AND  id<>'" + data["Id"] + @"' and ISNULL(EntityId,'" + data["EntityId"] + @"')='" + data["EntityId"] + @"' ";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private bool CheckNotNullEntityCombination(Dictionary<string, object> data)
        {
            try
            {
                string _sql = "";

                _sql = @"SELECT * FROM [dbo].[MachineBudget] where id<>'" + data["Id"] + "' and ArticleId='" + data["ArticleId"] + "' AND PlantId='" + data["PlantId"] + "' AND EntityId='" + data["EntityId"] + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckCombination(data);

                    if (IsDuplicateEntryAllowed)
                    {

                        var IsDuplicateEntityEntryAllowed = CheckEntityCombination(data);
                        if (!IsDuplicateEntityEntryAllowed)
                        {
                            throw new Exception("All Entity has been taken...");
                        }

                        DataSet dsMaster;
                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[MachineBudget] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                        string _Id = "";

                        #region data update
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MachineBudget", out _Id);

                            data["Id"] = "MB" + _Id;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                            _Id = data["Id"].ToString();
                            EditRow(dsMaster.Tables[0].Rows[0], data);
                        }
                        #endregion data update

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    else
                    {
                        throw new Exception("Selected combination already exists...");
                    }
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost]
        public JsonResult Edit(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckCombination(data);

                    if (IsDuplicateEntryAllowed)
                    {

                        var IsDuplicateEntityEntryAllowed = CheckEntityCombination(data);
                        if (!IsDuplicateEntityEntryAllowed)
                        {
                            throw new Exception("All Entity has been taken...");
                        }

                        DataSet dsMaster;
                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[MachineBudget] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                        string _Id = "";

                        #region data update
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MachineBudget", out _Id);

                            data["Id"] = "MB" + _Id;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                            _Id = data["Id"].ToString();
                            EditRow(dsMaster.Tables[0].Rows[0], data);
                        }
                        #endregion data update

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    else
                    {
                        throw new Exception("Selected combination already exists...");
                    }
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
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
            if (sourceData["EntityId"].ToString() == "ALL")
            {
                dr["EntityId"] = DBNull.Value;
            }
            else
            {
                dr["EntityId"] = sourceData["EntityId"].ToString();
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
            if (sourceData["EntityId"].ToString() == "ALL")
            {
                dr["EntityId"] = DBNull.Value;
            }
            else
            {
                dr["EntityId"] = sourceData["EntityId"].ToString();
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpGet, Authorize]
        public ActionResult GetMachineBudgetLevel(string plantId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT MachineBudgetLevel  FROM [SCS].[PlantConfig] Where PlantId='"+ plantId + "'";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMachineBudgetByArticle(string ArticleId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT MB.*,MM.UserName Material,P.UserName Plant,ISNULL(E.UserName,'ALL') Entity,MMA.StandardName Article    
                            FROM [dbo].[MachineBudget] MB
                            LEFT JOIN ORG.Plant P ON P.Id=MB.PlantId
                            LEFT JOIN ORG.Entity E ON E.Id=MB.EntityId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MB.ArticleId
                            LEFT JOIN MST.MaterialMaster  MM ON MM.Id=MMA.MaterialMasterId
                            Where MB.ArticleId='" + ArticleId + "'";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteBudgetData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteBudgetData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[MachineBudget] WHERE Id = '" + id + "'";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        [HttpGet, Authorize]
        public ActionResult GetMachineBudgetIndexReport(ReportFormat reportFormat)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Machine Budget";
            var workbook = GetMachineBudgetIndexReportWorkSheet();
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetMachineBudgetIndexReportWorkSheet()
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "MachineBudget";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetMachineBudgetData();

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Company", 25, ExcelHAlign.HAlignLeft);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 25, ExcelHAlign.HAlignLeft);
            int ColBulletinName = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 25, ExcelHAlign.HAlignLeft);
            int ColAlternativeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Master", 25, ExcelHAlign.HAlignLeft);
            int ColProductMaster = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 48, ExcelHAlign.HAlignLeft);
            int ColSizeGroup = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Prod.Machine Qty", 18, ExcelHAlign.HAlignLeft);
            int ColBuyer = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sample Machine Qty", 18, ExcelHAlign.HAlignLeft);
            int ColBuyerItemRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Training Machine Qty", 18, ExcelHAlign.HAlignLeft);
            int ColOwnStyleRefNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rent Machine Qty", 18, ExcelHAlign.HAlignLeft);
            int ColProcess = COL;
            COL++;

            endCol = COL;
            #endregion Headers

            var startRow = 0;

            int RowIndex = ROW;
            startRow = ROW;
            ROW++;
            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColId].Text = data.Rows[i]["Company"].ToString();
                sheet[ROW, ColBulletinName].Text = data.Rows[i]["Plant"].ToString();
                sheet[ROW, ColAlternativeName].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColProductMaster].Text = data.Rows[i]["MaterialMaster"].ToString();
                sheet[ROW, ColSizeGroup].Text = data.Rows[i]["Article"].ToString();

                sheet.Range[ROW, ColBuyer].Number =OTSBD.clsStaticInfo.dbl(data.Rows[i]["ProductionMachineQty"].ToString());
                sheet.Range[ROW, ColBuyer].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColBuyerItemRefNo].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["SampleMachineQty"].ToString());
                sheet.Range[ROW, ColBuyerItemRefNo].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColOwnStyleRefNo].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["TrainingMachineQty"].ToString());
                sheet.Range[ROW, ColOwnStyleRefNo].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet[ROW, ColProcess].Number = OTSBD.clsStaticInfo.dbl(data.Rows[i]["RentMachineQty"].ToString());
                sheet.Range[ROW, ColProcess].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyHeader(ref sheet, endCol, "Machine Budget", identity.CompanyId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetMachineBudgetData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Select C.UserName Company,P.UserName Plant,E.UserName Entity, MM.UserName MaterialMaster,MMA.ShortName Article
                                ,MB.ProductionMachineQty,MB.SampleMachineQty,MB.TrainingMachineQty,MB.RentMachineQty
                                from MachineBudget MB
                                Left join MSt.MaterialMasterArticle MMA ON MMA.Id=MB.ArticleId
                                Left join MSt.MaterialMaster MM ON MM.Id=MMA.MaterialMasterId
                                Left join ORG.Plant P ON P.Id=MB.PlantId
                                Left join ORG.Company C ON C.Id=P.CompanyId
                                Left join ORG.Entity E ON E.Id=MB.EntityId
                                Where MM.CompanyGroupId='"+ identity.CompanyGroupId + "' Order by C.UserName,p.UserName";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //#endregion
    }
}
