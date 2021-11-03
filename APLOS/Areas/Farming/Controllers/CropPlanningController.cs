#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

using Library.Service.Helpers;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Net.Http;

using Library.Model.IE;
using Library.Service.IEnumerable;
using Library.Model.Enums;
using Syncfusion.XlsIO;

#endregion Using

namespace Aplos.Areas.Farming.Controllers
{
    public class CropPlanningController : BaseController
    {
        string TableName = "TRN.CropPlanning";
        string TableName1 = "TRN.CropPlanningChild";
    


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public CropPlanningController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       


    
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM "+ TableName +"  "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult geticsmaster()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,Name AS Text FROM [MST].[ICSMaster]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult getcrop()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName AS Text FROM [MST].[CropMaster]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult getcroptype(string CropId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct ct.Id as Value,ct.UserName AS Text from HKP.CropType ct inner join MST.CropTypeMaster ctt on ct.Id=ctt.CropTypeId inner join MST.CropMaster cm on ctt.CropMasterId='"+ CropId + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getlandstatus(string CropTypeId, string CropId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct lc.Id, lc.UserName AS LandStatus, ct.AverageOutput from HKP.LandCategory lc inner join MST.CropTypeMaster ct on ct.LandCategoryId=lc.Id and ct.CropTypeId='"+ CropTypeId + "' and ct.CropMasterId='"+ CropId + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getfarmingcategory()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName as Text from HKP.FarmingCategory "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getvillage(string ICSMasterID)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct v.Id as Value,v.UserName as Text from HKP.Village v inner join MST.FarmerMaster fm on fm.VillageId=v.Id inner join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id and fmp.ICSMasterId='"+ ICSMasterID + "' "), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getfarmer(string ICSMasterID, string LandStatusId, string VillageId)
        {
            
            string sql;
          if (VillageId==null)
            {
                 sql = @"select distinct fm.Id as Value,fm.FarmerName as Text from MST.FarmerMaster fm inner join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id inner join MST.ICSMaster icm on fmp.ICSMasterId='" + ICSMasterID + @"' and fmp.PlotStatus='" + LandStatusId + @"'
                  order by Text";
            }
            else
            {
                 sql = @"select distinct fm.Id as Value,fm.FarmerName as Text from MST.FarmerMaster fm inner join MST.FarmerMasterPlot fmp on fmp.FarmerMasterId=fm.Id inner join MST.ICSMaster icm on fmp.ICSMasterId='" + ICSMasterID + @"' and fmp.PlotStatus='" + LandStatusId + @"'
                                                                            inner join MST.FarmerMaster fmm on fmm.VillageId='"+ VillageId + @"' order by Text";
            }
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public JsonResult getfarmerfather(string FarmerID)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct Id as Value, FarmerFatherHusbandName as Text from MST.FarmerMaster where Id='" + FarmerID + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getfarmerregistrationid(string FarmerFatherID)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct Id as Value, FarmerRegistrationID as Text, TotalArea from MST.FarmerMaster where Id='" + FarmerFatherID + "' "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getfarmerplot(string FarmerMasterId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct fmp.Id as Value,fmp.PlotNameNo as Text from MST.FarmerMasterPlot fmp inner join MST.FarmerMaster fm on fmp.FarmerMasterId='"+ FarmerMasterId + "' order by fmp.PlotNameNo "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getplotstatus(string FarmerPlotId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct lc.Id,lc.UserName as PlotStatus,fmp.PlotArea from HKP.LandCategory lc inner join MST.FarmerMasterPlot fmp on lc.Id=fmp.PlotStatus and fmp.Id='" + FarmerPlotId + "' "), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getplanquantity(decimal AverageOutput, decimal CropArea, decimal ProductivityIndex)
        {

            decimal Value;
            if (CropArea != 0 || AverageOutput != 0  || ProductivityIndex != 0)
            {
                 Value = CropArea * AverageOutput * ProductivityIndex;
               
            }
            else
            {
                throw new Exception("Crop Area, Average Output and Productivity Index are required to calculate Planned Quantity");
               
            }
            var jsondata = Json(Convert.ToString(Value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from TRN.CropPlanning where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           

            string sql = @"select distinct cp.*,ics.Name as ICSMaster,FORMAT(cp.StartDate,'dd-MMM-yyyy') as CPStartDate, FORMAT(cp.CloseDate,'dd-MMM-yyyy') as CPCloseDate
                                                 from TRN.CropPlanning cp left join MST.ICSMaster ics on cp.ICSMasterID=ics.Id WHERE " + strkey + " order by cp.UserName ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CropPlanning", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
             
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Year='" + data["Year"] + "' AND  Id<>'" + data["Id"] + "' and Season='" + data["Season"] + "' and UserName='" + data["UserName"] + "' and StartDate='" + data["StartDate"] + "' and CloseDate='" + data["CloseDate"] + "' and ICSMasterID='" + data["ICSMasterID"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Crop Planning already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "CP" + GetPK();
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
          

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where CropPlanningMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Crop Planning Child");
                    }
                }

                // ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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



        // *************** Crop Planning Child Tab ***************************

        private string GetPKC()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CropPlanningChild", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult SaveCropPlanningChild(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName1, out _Id);

                    data["Id"] = "CPC" + GetPKC();
                    AddNewRowCropPlanningChild(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRowCropPlanningChild(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        private void AddNewRowCropPlanningChild(DataTable dt, Dictionary<string, object> sourceData)
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

        private void EditRowCropPlanningChild(DataRow dr, Dictionary<string, object> sourceData)
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

        [Authorize, HttpPost]
        public ActionResult GetCropPlanningChild(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from TRN.CropPlanningChild where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult GetListCropPlanningChild(string CropPlanningMasterId)
        {

            string sql = @"select top 100 * from (select distinct cpc.*,cm.UserName as Crop,ctt.UserName as CropType,fm.FarmerName as Farmer,fmp.PlotNameNo as Plot,lc.Id as LcId, lc.UserName AS LandStatus,lc.Id as LcIdd, lc.UserName AS PlotStatus,
                                                  v.Id as VId,v.UserName as Village,ct.Id as CtId, ct.AverageOutput,fc.UserName as FarmingCategory
                                                  from TRN.CropPlanningChild cpc left join MST.CropMaster cm on cm.Id=cpc.CropId
												  left join HKP.FarmingCategory fc on fc.Id=cpc.FarmerCategoryId
                                                  left join MST.CropTypeMaster ct on ct.Id=cpc.CropTypeId
												  left join HKP.CropType ctt on ctt.Id=cpc.CropTypeId
												  left join MST.FarmerMaster fm on fm.Id=cpc.FarmerId
												  left join MST.FarmerMasterPlot fmp on fmp.Id=cpc.FarmerPlotId
												  left join HKP.LandCategory lc on ct.LandCategoryId=lc.Id and ct.Id=cpc.CropTypeId
												  left join MST.FarmerMasterPlot fmpp on lc.Id=fmpp.PlotStatus and fmpp.Id=cpc.FarmerPlotId
												  left join HKP.Village v on fm.VillageId=v.Id
												  inner join MST.FarmerMaster on fm.Id=fmp.FarmerMasterId
												  inner join TRN.CropPlanning cp on fmp.ICSMasterId=cp.ICSMasterID
                                                  where CropPlanningMasterId= '" + CropPlanningMasterId + "') AS TEMP order by Crop";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteCropPlanningChild(string Id)
        {
            try
            {
                string sql = @" delete from TRN.CropPlanningChild where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Crop Planning Child deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //#region Reports for Crop Planning

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpGet, Authorize]
        public ActionResult GetCropPlanningPrintReport(ReportFormat reportFormat, string CropPlanningPrintId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Crop Planning " + CropPlanningPrintId + "";
            var workbook = GetCropPlanningReportWorkSheet(CropPlanningPrintId);
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

        private IWorkbook GetCropPlanningReportWorkSheet(string CropPlanningPrintId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "CropPlanning";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetCropPlanningReportDataByCropPlanningId(CropPlanningPrintId);
            if (data.Rows.Count > 0)
            {
                int ColYearHeader = 1;
                int ColYearEnd;
                int ColSeasonHeader;
                int ColSeasonEnd;
                int ColSeason;
                int ColUserNameHeader;
                int ColUserNameEnd;
                int ColUserName;
                int ColStartDateHeader = 1;
                int ColStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColYearHeader, "Year", 12, ExcelHAlign.HAlignLeft);
                ColYearHeader++;
                ColYearEnd = ColYearHeader + 1;
                sheet.Range[ROW, ColYearHeader, ROW, ColYearEnd].Text = data.Rows[0]["Year"].ToString();
                sheet.Range[ROW, ColYearHeader, ROW, ColYearEnd].Merge();
                sheet.Range[ROW, ColYearHeader, ROW, ColYearEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColYearHeader, ROW, ColYearEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColYearEnd++;

                ColSeasonHeader = ColYearEnd;
                SetHeaderTextTop(ref sheet, ROW, ColSeasonHeader, "Season", 20, ExcelHAlign.HAlignLeft);
                ColSeasonHeader++;
                ColSeasonEnd = ColSeasonHeader + 1;
                ColSeason = ColSeasonHeader;
                sheet.Range[ROW, ColSeason, ROW, ColSeasonEnd].Text = data.Rows[0]["Season"].ToString();
                sheet.Range[ROW, ColSeason, ROW, ColSeasonEnd].Merge();
                sheet.Range[ROW, ColSeason, ROW, ColSeasonEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColSeason, ROW, ColSeasonEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColSeasonEnd++;

                ColUserNameHeader = ColSeasonEnd;
                SetHeaderTextTop(ref sheet, ROW, ColUserNameHeader, "User Name", 20, ExcelHAlign.HAlignLeft);
                ColUserNameHeader++;
                ColUserNameEnd = ColUserNameHeader + 1;
                ColUserName = ColUserNameHeader;
                sheet.Range[ROW, ColUserName, ROW, ColUserNameEnd].Text = data.Rows[0]["UserName"].ToString();
                sheet.Range[ROW, ColUserName, ROW, ColUserNameEnd].Merge();
                sheet.Range[ROW, ColUserName, ROW, ColUserNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColUserName, ROW, ColUserNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;


                SetHeaderTextTop(ref sheet, ROW, ColStartDateHeader, "Start Date", 12, ExcelHAlign.HAlignLeft);
                ColStartDateHeader++;
                ColStartDateEnd = ColStartDateHeader + 1;
                int ColStartDate = ColStartDateHeader;
                sheet.Range[ROW, ColStartDateHeader, ROW, ColStartDateEnd].Text = data.Rows[0]["CPStartDate"].ToString();
                sheet.Range[ROW, ColStartDateHeader, ROW, ColStartDateEnd].Merge();
                sheet.Range[ROW, ColStartDateHeader, ROW, ColStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColStartDateHeader, ROW, ColStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColStartDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColStartDateEnd, "Close Date", 20, ExcelHAlign.HAlignLeft);
                ColStartDateEnd++;
                int ColCloseDate = ColStartDateEnd;
                int ColCloseDateEnd = ColStartDateEnd + 1;
                sheet.Range[ROW, ColCloseDate, ROW, ColCloseDateEnd].Text = data.Rows[0]["CPCloseDate"].ToString();
                sheet.Range[ROW, ColCloseDate, ROW, ColCloseDateEnd].Merge();
                sheet.Range[ROW, ColCloseDate, ROW, ColCloseDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColCloseDate, ROW, ColCloseDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColCloseDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColCloseDateEnd, "ICS Master", 20, ExcelHAlign.HAlignLeft);
                ColCloseDateEnd++;
                int ColICSMaster = ColCloseDateEnd;
                int ColICSMasterEnd = ColCloseDateEnd + 1;
                sheet.Range[ROW, ColICSMaster, ROW, ColICSMasterEnd].Text = data.Rows[0]["ICSMaster"].ToString();
                sheet.Range[ROW, ColICSMaster, ROW, ColICSMasterEnd].Merge();
                sheet.Range[ROW, ColICSMaster, ROW, ColICSMasterEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColICSMaster, ROW, ColICSMasterEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
            }

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Crop", 12, ExcelHAlign.HAlignLeft);
            int ColCrop = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Crop Type", 8, ExcelHAlign.HAlignLeft);
            int ColCropType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Farmer", 8, ExcelHAlign.HAlignRight);
            int ColFarmer = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Plot", 15, ExcelHAlign.HAlignLeft);
            int ColFarmerPlot = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Crop Area", 15, ExcelHAlign.HAlignLeft);
            int ColCropArea = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Productivity Index", 20, ExcelHAlign.HAlignLeft);
            int ColProductivityIndex = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Quantity", 11, ExcelHAlign.HAlignLeft);
            int ColPlanQuantity = COL;
            COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Farmer Category", 11, ExcelHAlign.HAlignRight);
            int ColFarmerCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 10, ExcelHAlign.HAlignRight);
            int ColRemarks = COL;
            ROW++;
            endCol = COL;
            #endregion Headers

            string CPCCrops = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (CPCCrops != data.Rows[i]["Crop"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        sheet.Range[RowIndex, ColCrop, ROW - 1, ColCrop].Merge();
                        sheet.Range[RowIndex, ColCrop, ROW - 1, ColCrop].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColCrop, ROW - 1, ColCrop].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    }
                    RowIndex = ROW;
                }

                sheet[ROW, ColCropArea].Number = clsStaticInfo.dbl(data.Rows[i]["CropArea"].ToString());
                sheet[ROW, ColCrop].Text = data.Rows[i]["Crop"].ToString();
                sheet[ROW, ColCropType].Text = data.Rows[i]["CropType"].ToString();
                sheet[ROW, ColFarmerPlot].Text = data.Rows[i]["FarmerPlot"].ToString();
                sheet[ROW, ColFarmer].Text = data.Rows[i]["Farmer"].ToString();
                sheet[ROW, ColProductivityIndex].Number = clsStaticInfo.dbl(data.Rows[i]["ProductivityIndex"].ToString());
                sheet[ROW, ColPlanQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["PlanQuantity"].ToString());
                sheet[ROW, ColFarmerCategory].Text = data.Rows[i]["FarmerCategory"].ToString();
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                CPCCrops = data.Rows[i]["Crop"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                sheet.Range[RowIndex, ColCrop, ROW - 1, ColCrop].Merge();
                sheet.Range[RowIndex, ColCrop, ROW - 1, ColCrop].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColCrop, ROW - 1, ColCrop].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "Crop Planning", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetCropPlanningReportDataByCropPlanningId(string CropPlanningPrintId)
        {
            var sql = @"select cp.*, format(cp.StartDate,'dd-MMM-yyyy') as CPStartDate, format(cp.CloseDate,'dd-MMM-yyyy') as CPCloseDate, ics.Name as ICSMaster, cpc.CropPlanningMasterId as CropPlanningMaster, cm.UserName as Crop
                                                    ,ct.UserName as CropType, fm.FarmerName as Farmer, fmp.PlotNameNo as FarmerPlot, cpc.CropArea, cpc.ProductivityIndex, cpc.PlanQuantity, fc.UserName as FarmerCategory
													,cpc.Remarks from TRN.CropPlanning cp left join TRN.CropPlanningChild cpc on cp.Id=cpc.CropPlanningMasterId
													left join MST.ICSMaster ics on ics.Id=cp.ICSMasterID
													left join MST.CropMaster cm on cm.Id=cpc.CropId
													left join HKP.CropType ct on ct.Id=cpc.CropTypeId
													left join MST.FarmerMaster fm on fm.Id=cpc.FarmerId
													left join MST.FarmerMasterPlot fmp on fmp.Id=cpc.FarmerPlotId
													left join HKP.FarmingCategory fc on fc.Id=cpc.FarmerCategoryId
                                                    where cp.Id = '" + CropPlanningPrintId + "'";

            return _sqlRepository.GetDataTable(sql);
        }
        
    }

}