using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Helpers;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.IE.Controllers
{
    public class VASReportController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOperationService _operationService;
        private readonly IOperationVariationService _operationStepService;
        private readonly IOperationTimeCaptureMasterService _ioperationtimecaptureservice;
        private readonly IOperationTimeCaptureDetailService _operationtimecapturedetailservice;

        public VASReportController(
            IOperationTimeCaptureMasterService operationTimeCaptureService
            , IOperationTimeCaptureDetailService operationtimecapturedetailservice
            , IOperationService operationService
            , IOperationVariationService operationStepService
            , ISqlRepository sqlRepository)
        {
            _operationStepService = operationStepService;
            _operationtimecapturedetailservice = operationtimecapturedetailservice;
            _operationService = operationService;
            _ioperationtimecaptureservice = operationTimeCaptureService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        // GET: IE/VASReport
        #region -- Pages
        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region Forhad Code
        [HttpPost, Authorize]
        public JsonResult GetSelectedDataRangeData(string FromDate, string ToDate)
        {
            string sql = "";
            sql = @"
                    SELECT 
                    convert(bit,0) AS Checked,  M.VasDescription,M.OperationVariationSystemId, M.Id,V.Code AS OperationCode,V.StandardName,
                    M.SPI,M.RPM,M.MachineAllowances,M.PersonalAllowances,CASE WHEN M.IsApproved = 1 THEN 'Approved' ELSE '' END Status
                    ,UPPER(APPR.ApprovedBy)ApprovedBy,convert(datetime, APPR.ApprovedDate)ApprovedDate,
                    UPPER(M.AddedBy)AddedBy,convert(datetime, M.AddedDate)AddedDate,
                    mma.ShortName AS Machine,mmax.ShortName AS MachineActual,M.VASQuantity,
                     v.TotalSAM AS OperationSAM,M.VASSAM,M.StandardSAM,M.Version,M.AdditionalAllowances,
                    ps.UserName AS ProductionSystem,M.FactorValue AS ProductionSystemAllowance,M.OriginalVideoName,   
                    O.UserName as Operation
					FROM [MST].[VASMaster] M
                    INNER JOIN [MST].[OperationVariation] V ON V.Id = M.OperationVariationSystemId
                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=v.ArticleId
                    LEFT JOIN mst.MaterialMasterArticle AS mmax ON mmax.Id=M.ArticleId
                    LEFT JOIN [MST].[Operation] O ON O.Id = V.OperationId
                    LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=M.ProductionSystemId
                    LEFT JOIN  [MST].[VASMaster] AS APPR ON  APPR.OperationVariationSystemId = M.OperationVariationSystemId AND m.Id=APPR.Id
				    AND APPR.Id=(SELECT TOP 1 Id FROM mst.VASMaster AS XV WHERE XV.OperationVariationSystemId = m.OperationVariationSystemId AND XV.IsApproved=1)
                    WHERE CAST(M.AddedDate AS DATE) BETWEEN '" + FromDate + "'  AND '" + ToDate + "'  AND isnull(M.Archive,0)=0  ORDER BY V.Code,M.Version";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedOperationTimeDetails(string VASMasterID)
        {
            string sql = "";
            sql = @"SELECT C.Id,VASMasterID,ElementID,E.ShortName AS ElementType,C.Sequence,
                    EC.UserName ElementCode,C.TMU,CT1,CT2,CT3,CT4,CT5,TimeAvg,Ratings,BasicTime,[Version]
                    FROM [MST].[VASChild] C
                    INNER JOIN HKP.ElementType E On E.Id = C.ElementTypeId
                    INNER JOIN HKP.ElementCode EC On EC.Id = C.ElementID
                    WHERE C.VASMasterID='" + VASMasterID + "'  ORDER BY C.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetVASReport(string ReportData)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + "VAS_REPORT.xlsx";
            var workbook = VASReportSheet(ReportData);


            string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + reportFileName);
            workbook.Version = ExcelVersion.Excel2013;
            workbook.SaveAs(fullPath);

            return Json(new { FileName = reportFileName, Error = false }, JsonRequestBehavior.AllowGet);
        }

        private IWorkbook VASReportSheet(string Id)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            var workbook = application.Workbooks.Create(1);
            workbook.Version = ExcelVersion.Excel2013;

            var _sheetName = "Video Analysis Report";
            IWorksheet sheet1 = workbook.Worksheets[0];
            sheet1.Name = _sheetName;

            IStyle HFontstyle = workbook.Styles.Add("NewStyle");


            //sheet1.Range["E3:J3"].Merge();


            int ROW = 6;
            int COL = 1;
            int STRATCOL = 1;


            sheet1[ROW, 1].Text = "**All durations are expressed in seconds";
            sheet1.Range[ROW, 1, ROW, 10].Merge();
            ROW++;

            sheet1[ROW, COL].Text = "Id";
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColId = COL;
            COL++;

            sheet1[ROW, COL].Text = "Opr. Var. Code";
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColOperationCode = COL;
            COL++;
            sheet1[ROW, COL].Text = "Opr.Var.Name";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColOperationVarName = COL;
            COL++;
            sheet1[ROW, COL].Text = "Operation";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColOperation = COL;
            COL++;
            sheet1[ROW, COL].Text = "Opt. Machine";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColMachine = COL;
            COL++;
            sheet1[ROW, COL].Text = "Actual Machine";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColMachineActual = COL;
            COL++;
            sheet1[ROW, COL].Text = "Stitch Code";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColMachineActualStitchCode = COL;
            COL++;
            sheet1[ROW, COL].Text = "Version";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColVersion = COL;
            COL++;
            sheet1[ROW, COL].Text = "Description";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColVasDescription = COL;
            COL++;
            sheet1[ROW, COL].Text = "Operator Name";
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColOperatorName = COL;
            COL++;
            sheet1[ROW, COL].Text = "SPI";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColSPI = COL;
            COL++;
            sheet1[ROW, COL].Text = "RPM";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColRPM = COL;
            COL++;
            sheet1[ROW, COL].Text = "Personal Allowance %";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColPersonalAllowances = COL;
            COL++;
            sheet1[ROW, COL].Text = "Machine Allowance %";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColMachineAllowances = COL;
            COL++;
            sheet1[ROW, COL].Text = "Additional Allowance %";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColAdditionalAllowances = COL;

            COL++;
            sheet1[ROW, COL].Text = "Production System";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColProductionSystem = COL;
            COL++;
            sheet1[ROW, COL].Text = "Production System Allowance %";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColProductionSystemAllowance = COL;
            COL++;
            sheet1[ROW, COL].Text = "Operation SPT";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColOperationSAM = COL;
            COL++;
            sheet1[ROW, COL].Text = "VAS SPT";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColVASSAM = COL;
            COL++;
            sheet1[ROW, COL].Text = "Standard SPT";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColStandardSAM = COL;

            COL++;
            sheet1[ROW, COL].Text = "Qty";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColVASQuantity = COL;

            COL++;



            sheet1[ROW, COL].Text = "Calculation Type";
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColCalculationType = COL;
            COL++;

            //sheet1[ROW, COL].Text = "Frequency";
            //sheet1[ROW, COL].ColumnWidth = 7;
            //int ColFrequency = COL;
            //COL++;







            sheet1[ROW, COL].Text = "Element Type";
            sheet1[ROW, COL].ColumnWidth = 10;
            int ColElementType = COL;
            COL++;

            sheet1[ROW, COL].Text = "Element";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 6;
            int ColSequence = COL;
            COL++;
            sheet1[ROW, COL].Text = "Element Code";
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColElementCode = COL;
            COL++;

            sheet1[ROW, COL].Text = "Element Name";
            sheet1[ROW, COL].ColumnWidth = 14;
            int ColElementName = COL;
            COL++;


            sheet1[ROW, COL].Text = "CT1 (Sec)";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColCT1 = COL;
            COL++;

            sheet1[ROW, COL].Text = "CT2 (Sec)";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColCT2 = COL;
            COL++;

            sheet1[ROW, COL].Text = "CT3 (Sec)";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColCT3 = COL;
            COL++;

            sheet1[ROW, COL].Text = "CT4 (Sec)";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColCT4 = COL;
            COL++;

            sheet1[ROW, COL].Text = "CT5 (Sec)";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColCT5 = COL;
            COL++;

            sheet1[ROW, COL].Text = "Time Avg";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColTimeAvg = COL;
            COL++;

            sheet1[ROW, COL].Text = "Ratings %";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColRatings = COL;
            COL++;

            sheet1[ROW, COL].Text = "Basic Time";
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColBasicTime = COL;
            COL++;
            sheet1[ROW, COL].Text = "Created By";
            sheet1[ROW, COL].ColumnWidth = 10;
            int ColAddedBy = COL;
            COL++;
            sheet1[ROW, COL].Text = "Creation Date";
            sheet1[ROW, COL].ColumnWidth = 16;
            int ColAddedDate = COL;
            COL++;
            sheet1[ROW, COL].Text = "Approved By";
            sheet1[ROW, COL].ColumnWidth = 10;
            int ColApprovedBy = COL;
            COL++;
            sheet1[ROW, COL].Text = "Approval Date";
            sheet1[ROW, COL].ColumnWidth = 16;
            int ColApprovedDate = COL;
            COL++;
            sheet1[ROW, COL].Text = "Approve Status";
            sheet1[ROW, COL].ColumnWidth = 7;
            int ColApproveStatus = COL;
            COL++;
            sheet1[ROW, COL].Text = "Remarks";
            sheet1[ROW, COL].ColumnWidth = 32;
            int ColRemarks = COL;

            int EndCol = COL;

            sheet1.Range[ROW, STRATCOL, ROW, EndCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet1.Range[ROW, STRATCOL, ROW, EndCol].CellStyle.Font.Color = ExcelKnownColors.Black;
            sheet1.Range[ROW, STRATCOL, ROW, EndCol].CellStyle.Font.Bold = true;
            sheet1.Range[ROW, STRATCOL, ROW, EndCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet1.Range[ROW, STRATCOL, ROW, EndCol].BorderAround(ExcelLineStyle.Hair);
            sheet1.Range[ROW, STRATCOL, ROW, EndCol].BorderInside(ExcelLineStyle.Hair);
            sheet1.IsGridLinesVisible = false;

            ROW++;

            string[] reportId = Id.Split(',');
            string inQuery = Id.Replace(",", "','");
            inQuery = "'" + inQuery + "'";

            string sql = @"SELECT 
                    M.Id,OV.Code AS OperationCode,OV.UserName AS OperationVariation, ov.TotalSAM AS OperationSAM,M.VASSAM,M.StandardSAM,
                    B.UserName AS  ProductionSystem, M.FactorValue ProductionSystemAllowance,OPT.UserName AS Operation,M.Remarks,
                    mma.ShortName AS Machine,mmax.ShortName AS MachineActual,M.VASQuantity,sc.UserName AS StitchCode,
                        CASE WHEN M.AvgMaxMin = 1 THEN 'AVG' WHEN M.AvgMaxMin = 2 THEN 'MAX' ELSE 'MIN' END CalculationType, M.Frequency,M.SPI,M.RPM,
                        M.MachineAllowances,M.PersonalAllowances,ElementID,E.ShortName AS ElementType, G.Code AS ElementCode, M.OperatorId,
                        G.UserName ElementName, C.TMU,CT1,CT2,CT3,CT4,CT5,m.AdditionalAllowances,M.VasDescription,C.Sequence
                    ,UPPER(APPR.ApprovedBy)ApprovedBy,convert(datetime, APPR.ApprovedDate)ApprovedDate,
                                        UPPER(M.AddedBy)AddedBy,convert(datetime, M.AddedDate)AddedDate,
                        TimeAvg,Ratings,BasicTime,M.Version,CASE WHEN M.IsApproved = 1 THEN 'Approved' Else CAST('' As VARCHAR(50)) END ApproveStatus
                      FROM[MST].[VASMaster] M
						join mst.OperationVariation OV ON OV.ID=M.OperationVariationSystemId
                    LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=OV.ArticleId
                    LEFT JOIN mst.MaterialMasterArticle AS mmax ON mmax.Id=M.ArticleId
                        LEFT JOIN hkp.StitchCode AS sc  ON sc.Id=isnull(mmax.StitchCodeId,mma.StitchCodeId)
						join mst.Operation OPT ON OPT.Id=OV.OperationId
                       INNER JOIN[HKP].[ProductionSystem] B ON B.Id = M.ProductionSystemId
                       INNER JOIN[MST].[VASChild] C ON C.VASMasterId = M.Id
                       LEFT JOIN[HKP].ElementCode G ON G.Id = C.ElementId
                       INNER JOIN HKP.ElementType E On E.Id = C.ElementTypeId
                        LEFT JOIN  [MST].[VASMaster] AS APPR ON  APPR.OperationVariationSystemId = M.OperationVariationSystemId AND m.Id=APPR.Id
				        AND APPR.Id=(SELECT TOP 1 Id FROM mst.VASMaster AS XV WHERE XV.OperationVariationSystemId = m.OperationVariationSystemId AND XV.IsApproved=1)

                        WHERE M.Id IN (" + inQuery + @")  AND isnull(M.Archive,0)=0 ORDER BY M.Id,C.Sequence";

            DataTable dtLocal = _sqlRepository.GetDataTable(sql);
            int StartRow = ROW;
            foreach (string val in reportId)
            {
                dtLocal.DefaultView.RowFilter = "Id='" + val + "'";
                DataTable dt = dtLocal.DefaultView.ToTable();


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sheet1[ROW, ColId].Text = dt.Rows[i]["Id"].ToString();
                    sheet1[ROW, ColOperationCode].Text = dt.Rows[i]["OperationCode"].ToString();
                    sheet1[ROW, ColMachine].Text = dt.Rows[i]["Machine"].ToString();
                    sheet1[ROW, ColMachineActual].Text = dt.Rows[i]["MachineActual"].ToString();
                    sheet1[ROW, ColMachineActualStitchCode].Text = dt.Rows[i]["StitchCode"].ToString();
                    sheet1[ROW, ColOperationVarName].Text = dt.Rows[i]["OperationVariation"].ToString();
                    sheet1[ROW, ColOperation].Text = dt.Rows[i]["Operation"].ToString();
                    sheet1[ROW, ColOperatorName].Text = dt.Rows[i]["OperatorId"].ToString();
                    sheet1[ROW, ColVasDescription].Text = dt.Rows[i]["VasDescription"].ToString();
                    sheet1[ROW, ColProductionSystem].Text = dt.Rows[i]["ProductionSystem"].ToString();
                    sheet1[ROW, ColOperationSAM].Number = clsStaticInfo.dbl(dt.Rows[i]["OperationSAM"].ToString());
                    sheet1[ROW, ColVASSAM].Number = clsStaticInfo.dbl(dt.Rows[i]["VASSAM"].ToString());
                    sheet1[ROW, ColStandardSAM].Number = clsStaticInfo.dbl(dt.Rows[i]["StandardSAM"].ToString());
                    //sheet1[ROW, ColFrequency].Number = clsStaticInfo.dbl(dt.Rows[i]["Frequency"].ToString());
                    sheet1[ROW, ColProductionSystemAllowance].Number = clsStaticInfo.dbl(dt.Rows[i]["ProductionSystemAllowance"].ToString());
                    sheet1[ROW, ColCalculationType].Text = dt.Rows[i]["CalculationType"].ToString();
                    //sheet1[ROW, ColFrequency].Number = clsStaticInfo.dbl(dt.Rows[i]["Frequency"].ToString());
                    sheet1[ROW, ColSPI].Number = clsStaticInfo.dbl(dt.Rows[i]["SPI"].ToString());
                    sheet1[ROW, ColRPM].Number = clsStaticInfo.dbl(dt.Rows[i]["RPM"].ToString());
                    sheet1[ROW, ColMachineAllowances].Number = clsStaticInfo.dbl(dt.Rows[i]["MachineAllowances"].ToString());
                    sheet1[ROW, ColPersonalAllowances].Number = clsStaticInfo.dbl(dt.Rows[i]["PersonalAllowances"].ToString());
                    sheet1[ROW, ColAdditionalAllowances].Number = clsStaticInfo.dbl(dt.Rows[i]["AdditionalAllowances"].ToString());
                    //sheet1[ROW, ColElementID].Number = clsStaticInfo.dbl(dt.Rows[i]["ElementID"].ToString());
                    sheet1[ROW, ColElementType].Text = dt.Rows[i]["ElementType"].ToString();
                    sheet1[ROW, ColElementCode].Text = dt.Rows[i]["ElementCode"].ToString();
                    sheet1[ROW, ColElementName].Text = dt.Rows[i]["ElementName"].ToString();
                    //sheet1[ROW, ColTMU].Number = clsStaticInfo.dbl(dt.Rows[i]["TMU"].ToString());
                    sheet1[ROW, ColCT1].Number = clsStaticInfo.dbl(dt.Rows[i]["CT1"].ToString());
                    sheet1[ROW, ColCT2].Number = clsStaticInfo.dbl(dt.Rows[i]["CT2"].ToString());
                    sheet1[ROW, ColCT3].Number = clsStaticInfo.dbl(dt.Rows[i]["CT3"].ToString());
                    sheet1[ROW, ColCT4].Number = clsStaticInfo.dbl(dt.Rows[i]["CT4"].ToString());
                    sheet1[ROW, ColCT5].Number = clsStaticInfo.dbl(dt.Rows[i]["CT5"].ToString());
                    sheet1[ROW, ColTimeAvg].Number = clsStaticInfo.dbl(dt.Rows[i]["TimeAvg"].ToString());
                    sheet1[ROW, ColRatings].Number = clsStaticInfo.dbl(dt.Rows[i]["Ratings"].ToString());
                    sheet1[ROW, ColBasicTime].Number = clsStaticInfo.dbl(dt.Rows[i]["BasicTime"].ToString());
                    sheet1[ROW, ColVersion].Number = clsStaticInfo.dbl(dt.Rows[i]["Version"].ToString());
                    sheet1[ROW, ColSequence].Number = clsStaticInfo.dbl(dt.Rows[i]["Sequence"].ToString());
                    sheet1[ROW, ColVASQuantity].Number = clsStaticInfo.dbl(dt.Rows[i]["VASQuantity"].ToString());


                    sheet1[ROW, ColApproveStatus].Text = dt.Rows[i]["ApproveStatus"].ToString();
                    sheet1[ROW, ColRemarks].Text = dt.Rows[i]["Remarks"].ToString();
                    sheet1[ROW, ColAddedBy].Text = dt.Rows[i]["AddedBy"].ToString();
                    if (dt.Rows[i]["AddedDate"].ToString() != "")
                        sheet1[ROW, ColAddedDate].DateTime = Convert.ToDateTime(clsStaticInfo.GetDateTime(dt.Rows[i]["AddedDate"].ToString()));
                    sheet1[ROW, ColApprovedBy].Text = dt.Rows[i]["ApprovedBy"].ToString();
                    if (dt.Rows[i]["ApprovedDate"].ToString() != "")
                        sheet1[ROW, ColApprovedDate].DateTime = Convert.ToDateTime(clsStaticInfo.GetDateTime(dt.Rows[i]["ApprovedDate"].ToString()));



                    if (dt.Rows[i]["Machine"].ToString() != dt.Rows[i]["MachineActual"].ToString())
                        sheet1[ROW, ColMachineActual].CellStyle.Interior.ColorIndex = ExcelKnownColors.Red;

                    sheet1.Range[ROW, STRATCOL, ROW, EndCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[ROW, STRATCOL, ROW, EndCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;
                }

                dt.Dispose();
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet1.UsedRange.NumberFormat = "#,##0.000";
            sheet1.Range[StartRow, ColRPM, ROW, ColRPM].NumberFormat = clsStaticInfo.NumberFormat();
            sheet1.Range[StartRow, ColSequence, ROW, ColSequence].NumberFormat = clsStaticInfo.NumberFormat();
            sheet1.Range[StartRow, ColMachineAllowances, ROW, ColMachineAllowances].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet1.Range[StartRow, ColPersonalAllowances, ROW, ColPersonalAllowances].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet1.Range[StartRow, ColAdditionalAllowances, ROW, ColAdditionalAllowances].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet1.Range[StartRow, ColProductionSystemAllowance, ROW, ColProductionSystemAllowance].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet1.Range[StartRow, ColVersion, ROW, ColVersion].NumberFormat = clsStaticInfo.NumberFormat(0);
            sheet1.Range[StartRow, ColAddedDate, ROW, ColAddedDate].NumberFormat = "dd-MMM-yyyy H:mm:ss";
            sheet1.Range[StartRow, ColApprovedDate, ROW, ColApprovedDate].NumberFormat = "dd-MMM-yyyy H:mm:ss";
            sheet1.Range[StartRow - 1, 1, ROW, EndCol].WrapText = true;
            sheet1.UsedRange.CellStyle.Font.Size = 8;

            ROW--;
            IListObject table = sheet1.ListObjects.Create("Table1", sheet1[clsStaticInfo.GetxlsCol(1) + (StartRow - 1).ToString() + ":" + clsStaticInfo.GetxlsCol(EndCol) + (ROW).ToString()]);
            table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;


            HFontstyle.Font.Bold = true;
            HFontstyle.Font.Size = 15;
            sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;


            ReportUtility reportUtility = new ReportUtility();
            reportUtility.CompanyGroupHeader(ref sheet1, 1, "Video Analysis Report", identity.CompanyGroupId);

            reportUtility.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);
            sheet1[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet1.Range[1, 1, 5, EndCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            return workbook;
        }
        #endregion
    }
}