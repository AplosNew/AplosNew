#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Materials.Controllers
{
    public class UtilityTransactionReportController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public UtilityTransactionReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ReportView()
        {
            return View();
        }

        // [HttpGet, Authorize]
        // public ActionResult getFilters()
        // {
        //     try
        //     {
        //         var sql = @"select UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],MAX(CONVERT(varchar(5),UT.AddedDate,108)) [Time],UM.UtilityGroup [Group],UM.UtilitySubGroup SubGroup,UM.UtilityCategory Category
        //,UM.UtilitySubCategory SubCategory,UM.Item,EI.EmployeeName ResponsiblePerson 
        //,format(UT.AddedDate,'dd-MMM-yyyy')AddedDate,UT.Quantity,UT.Reading,UT.Remarks
        //from UtilityTransaction UT
        //left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
        //left join EmployeeInformation EI on EI.SystemId=UM.ResponsiblePersonId
        //group by UT.Id,UT.Date,UT.AddedDate,UM.UtilityGroup,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory
        //,UM.Item,EI.EmployeeName,UT.Quantity,UT.Reading,UT.Remarks";

        //         return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        //     }
        //     catch (Exception e)
        //     {
        //         throw e;
        //     }
        // }

        [HttpGet, Authorize]
        public JsonResult GetUserGroup()
        {

            var sql = @"SELECT Id Value , UserName Text FROM HKP.UtilityGroup where Active = 1 ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getUtilityTransactionData(string ToDate, string FromDate , string UtilityGroupId)
        {
            var str = @"declare @Fromdate date = '" + FromDate +@"' 
, @Todate date = '" + ToDate + @"'  
,@UtilityGroupid  Varchar(20)= '" + UtilityGroupId + @"' ;
 
select format(UT.Date,'yyyy-MM-dd') TransactionDate, UT.UtilityMasterId , UM.UserName UtilityMasterName,isnull(UMSS.Id,'') InputSouceId,isnull(UMSS.UserName,'') InputSouceName,   isnull(UT.MultiplyingFactor,0)*UT.Quantity TransactionQuantity , isnull(UMS.Reading,0) SumOfChild
,(isnull(UT.MultiplyingFactor,0)*UT.Quantity) - isnull(UMS.Reading,0) NetQty , UG.UserName UtilityGroup
,Rate = isnull((select Top 1 Rate from UtilityDetail where EffectiveDate Between @Fromdate and @Todate and UtilityMasterId = UM.Id Order by AddedDate desc),0)
,Um.UtilityCategory,UM.UtilitySubCategory , UOM.UserName UOM , UT.Quantity , UT.MultiplyingFactor , UT.Remarks ,UT.Id , ET.UserName Entity,UT.LastReading,UT.Reading CurrentReading 
from UtilityTransaction UT
left join (select format(UT.Date,'yyyy-MM-dd') [Date], UT.InPutSourceId , UMSS.UserName InputSouce, sum(isnull(UT.MultiplyingFactor,0)*UT.Quantity) Reading 
			from UtilityTransaction UT 
			left join UtilityMaster UM on UM.Id = UT.UtilityMasterId 
			left join UtilityMaster UMSS on UMSS.Id = UT.InPutSourceId
			Group by [Date],UT.InPutSourceId ,UMSS.UserName ) UMS on UMS.Date = UT.Date and UMS.InPutSourceId = UT.UtilityMasterId
			left join UtilityMaster UM on UM.Id	= UT.UtilityMasterId
			left join org.Entity ET on ET.Id = UM.EntityId
			left join UtilityMaster UMSS on UMSS.Id = UT.InPutSourceId 
			left join hkp.UtilityGroup UG on UG.Id = UM.UtilityGroupId
            left join [SCS].[UnitOfMeasurement] UOM on UOM.Id = UT.UoMId
			where UT.[Date] Between @Fromdate and  @Todate and UM.UtilityGroupId = @UtilityGroupid and UM.Active = 1";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

            
        [HttpPost, Authorize]
        public ActionResult GetUtilityTransactionReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            { 
                string fileName = "";
                fileName = UtilityTransactionReportxlx(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string UtilityTransactionReportxlx(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "UtilityTransactionReport";
                sheet = workbook.Worksheets[0];
                //DataTable data;
                //UtilityTransactionReportSQL(ToDate,FromDate, out data);
                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "TransactionDate";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "UtilityMasterName";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColMasterName = COL;
                COL++;

                sheet[ROW, COL].Text = "InputSouceName";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColSourceName = COL;
                COL++;
                sheet[ROW, COL].Text = "TransactionQuantity";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColTrnQty = COL;
                COL++;

                sheet[ROW, COL].Text = "SumOfChild";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColSumOfChild = COL;
                COL++;

                sheet[ROW, COL].Text = "NetQty";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColNetQty = COL;
                COL++;

                sheet[ROW, COL].Text = "UtilityGroup";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Rate";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColRate = COL;
                COL++;

               

               
                sheet[ROW, COL].Text = "UtilityCategory";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "UtilitySubCategory";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColSunCategory = COL;
                COL++;
                sheet[ROW, COL].Text = "UOM";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColUOM = COL;
                COL++;
                sheet[ROW, COL].Text = "Quantity";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColQuantity = COL;
                COL++;

                sheet[ROW, COL].Text = "Multiplying Factor";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColMultiplyingFactor = COL;
                COL++;

                sheet[ROW, COL].Text = "Last Reading";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColLastReading = COL;
                COL++;

               
                sheet[ROW, COL].Text = "Current Reading";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColCurrentReading = COL;
                

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, ColDate].Text = data[i]["TransactionDate"].ToString(); 
                    sheet[ROW, ColEntity].Text = data[i]["Entity"].ToString(); 
                    sheet[ROW, ColMasterName].Text = data[i]["UtilityMasterName"].ToString(); 
                    sheet[ROW, ColSourceName].Text = data[i]["InputSouceName"].ToString();
                    sheet[ROW, ColTrnQty].Number = clsStaticInfo.dbl(data[i]["TransactionQuantity"].ToString());
                    sheet[ROW, ColTrnQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColSumOfChild].Number = clsStaticInfo.dbl(data[i]["SumOfChild"].ToString());
                    sheet[ROW, ColSumOfChild].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColNetQty].Number = clsStaticInfo.dbl(data[i]["NetQty"].ToString());
                    sheet[ROW, ColNetQty].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColGroup].Text = data[i]["UtilityGroup"].ToString();
                    sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data[i]["Rate"].ToString());
                    sheet[ROW, ColRate].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                    sheet[ROW, ColCategory].Text = data[i]["UtilityCategory"].ToString();
                    sheet[ROW, ColSunCategory].Text = data[i]["UtilitySubCategory"].ToString();
                    sheet[ROW, ColUOM].Text = data[i]["UOM"].ToString();
                    sheet[ROW, ColQuantity].Number = clsStaticInfo.dbl(data[i]["Quantity"].ToString());
                    sheet[ROW, ColQuantity].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColMultiplyingFactor].Number = clsStaticInfo.dbl(data[i]["MultiplyingFactor"].ToString());
                    sheet[ROW, ColMultiplyingFactor].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColLastReading].Number = clsStaticInfo.dbl(data[i]["LastReading"].ToString());
                    sheet[ROW, ColLastReading].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet[ROW, ColCurrentReading].Number = clsStaticInfo.dbl(data[i]["CurrentReading"].ToString());
                    sheet[ROW, ColCurrentReading].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Utility Transaction Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UtilityTransactionReportSQL(string ToDate, string FromDate, out DataTable data)
        {
            try
            {
                string strSQL = @"select UT.Id,FORMAT(UT.Date,'dd-MMM-yyyy') [Date],MAX(CONVERT(varchar(5),UT.AddedDate,108)) [Time],UG.UserName [Group],UM.UtilitySubGroup SubGroup,UM.UtilityCategory Category,UM.Item
							,UM.UtilitySubCategory SubCategory,UM.Item,EI.EmployeeName ResponsiblePerson 
							,format(UT.AddedDate,'dd-MMM-yyyy')AddedDate,UT.Quantity,UT.Reading,isnull(UT.Remarks,'')Remarks
                            ,Amount=isnull(UT.Quantity*(SELECT TOP(1) Rate FROM dbo.UtilityDetail  WHERE EffectiveDate between '" + FromDate + @"' and '" + ToDate + @"' AND UtilityMasterId=UT.UtilityMasterId ORDER BY EffectiveDate),0)
                            ,isnull(UT.MultiplyingFactor,0) MultiplyingFactor
							,FinalQuantity=isnull(UT.MultiplyingFactor,0)*UT.Quantity
							,ET.UserName Entity , UOM.UserName UOM 
                            ,case when  UD.Rate is null then 0 else UD.Rate end Rate
							,case when  UD.EffectiveDate is null then '' else format(UD.EffectiveDate,'dd-MMM-yyyy') end EffectiveDate
							,PT.UserName PartyName , RS.EmployeeName ResponsiblePerson , AD.EmployeeName [Admin] , UM.EntryLegDays , 
							case when um.Active = 1 then 'Active' else 'InActive' end Status
							from UtilityTransaction UT
							left join UtilityMaster UM on UM.Id=UT.UtilityMasterId
							left join EmployeeInformation EI on EI.SystemId=UM.ResponsiblePersonId
                            left join HKP.UtilityGroup UG on UG.Id=UM.UtilityGroupId
							left join org.Entity ET on ET.Id = UM.EntityId
							left join [SCS].[UnitOfMeasurement] UOM on UOM.Id = UM.UoMId
							left join UtilityDetail UD on UD.UtilityMasterId = UM.Id
							left join hkp.Party PT on PT.Id = UM.PartyId
							left join EmployeeInformation RS on RS.SystemId = UM.ResponsiblePersonId
							left join EmployeeInformation AD on AD.SystemId = UM.AdminId
                             where UT.Date between '" + FromDate + @"' and '" + ToDate + @"'
                             group by UT.Id,UT.Date,UT.AddedDate,UM.UserName,UM.UtilitySubGroup,UM.UtilityCategory,UM.UtilitySubCategory
							,UM.Item,EI.EmployeeName,UT.Quantity,UT.Reading,UT.Remarks,UG.UserName,UT.MultiplyingFactor,UT.UtilityMasterId , ET.UserName , UOM.UserName
							,UD.Rate , format(UD.EffectiveDate,'dd-MMM-yyyy') ,PT.UserName , PT.UserName , RS.EmployeeName ,  AD.EmployeeName , UM.EntryLegDays
							,um.Active";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }


    }
}