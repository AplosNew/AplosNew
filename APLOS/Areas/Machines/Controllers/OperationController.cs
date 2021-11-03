#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Machines;
using Library.Service.Helpers;
using Library.Service.Machines;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Machines.Controllers
{
    public class OperationController : BaseController
    {
        #region -- Constructor

        private readonly IOperationService _operationService;
        private readonly IOperationFgComponentService _operationFgComponentService;
        private readonly IOperationProcessService _operationProcessService;

        private readonly ISqlRepository _sqlRepository;


        public OperationController(IOperationService operationService
            , IOperationFgComponentService operationFgComponentService
            , IOperationProcessService operationProcessService
            , ISqlRepository sqlRepository
            
            )
        {
            _operationService = operationService;
            _operationFgComponentService = operationFgComponentService;
            _operationProcessService = operationProcessService;
            _sqlRepository = sqlRepository;


        }

        #endregion -- Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region Get List

        /// <summary>
        /// use in operation, entity operation settings
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationUtilityData(string operationId)
        {
            return Json(_operationService.GetOperationUtilityData(operationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationListByProcess(GridParameter parameters, string processId)
        {
            return Json(_operationService.GetOperationListByProcess(parameters, processId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationProcessList(string operationId)
        {
            return Json(_operationProcessService.Query(operationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetArticleListByMaterialMaster(GridParameter parameters, string materialMasterId)
        {
            return Json(_operationService.GetArticleListByMaterialMaster(parameters, materialMasterId), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet, Authorize]
        public ActionResult GetOperationFGComponentList(string operationId)
        {
            return Json(_operationFgComponentService.Query(operationId), JsonRequestBehavior.AllowGet);
        }

        #endregion Get List

        #region -- Operations

        [Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetOperationCbo(string subprocessid)
        {
            return Json(_operationService.GetOperationCbo(subprocessid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_operationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Operation operation
            , IEnumerable<OperationProcess> processList
            , IEnumerable<OperationFgComponent> operationFgComponent
            , IEnumerable<OperationAttribute> attributeList
            , IEnumerable<OperationAttributeValue> valueList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            operation.CompanyGroupId = identity.CompanyGroupId;
            _operationService.InsertGraph(operation, processList, operationFgComponent, attributeList, valueList);
            return Json(new { Sequence = _operationService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Operation operation
      , IEnumerable<OperationProcess> processList
            , IEnumerable<OperationFgComponent> operationFgComponent
            , IEnumerable<OperationAttribute> attributeList
            , IEnumerable<OperationAttributeValue> valueList)
        {
            _operationService.UpdateGraph(operation, processList, operationFgComponent, attributeList, valueList);
            return Json(new { Sequence = _operationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _operationService.DeleteGraph(id);
            return Json(new { Sequence = _operationService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        #region Attribute

        [HttpGet, Authorize]
        public ActionResult GetOperationAttribute(string operationId)
        {
            return Json(_operationService.GetOperationAttributeList(operationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationAttributeListForSubOperation(string operationId)
        {
            return Json(_operationService.GetOperationAttributeListForSubOperation(operationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAttributeSequence(string operationId)
        {
            return Json(_operationService.GetAttributeSequence(operationId), JsonRequestBehavior.AllowGet);
        }

        #endregion Attribute

        #region Operation Attribute Value

        [Authorize, HttpGet]
        public JsonResult GetOperationAttributeValueList(string operationId)
        {
            return Json(_operationService.GetOperationAttributeValueList(operationId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetValueSequence(string operationAttributeId)
        {
            return Json(_operationService.GetValueSequence(operationAttributeId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetValueListByAttributeId(GridParameter parameters, string attributeId)
        {
            return Json(_operationService.GetValueListByAttributeId(parameters, attributeId), JsonRequestBehavior.AllowGet);
        }


        #endregion Operation Attribute Value


        #region Report
        [HttpGet, Authorize]
        public ActionResult GetOperationReportExcel()   //bool checkbox
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
                IWorkbook workbook = OperationReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Operation Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        //Operation Report
        private DataTable GetOperationReportData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"               
   

             SELECT O.Id
	       , Process=STUFF((SELECT DISTINCT ',' + P.UserName FROM [MST].[OperationProcess] AS OPMT
			                    LEFT JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
			                    WHERE OPMT.OperationId=O.Id
			                    GROUP BY P.UserName
			                    FOR XML PATH ('')
			                    ),1,1,'')
		    , FGComponent=STUFF((SELECT DISTINCT ',' + FG.UserName FROM [MST].[OperationFgComponent] AS OPFC
			                    LEFT JOIN [HKP].[FGComponent] AS FG ON OPFC.FGComponentId=FG.Id
			                    WHERE OPFC.OperationId=O.Id
			                    GROUP BY FG.UserName
			                    FOR XML PATH ('')
			                    ),1,1,'')
								
		 , OperationAttribute=STUFF((SELECT DISTINCT ',' + OA.UserName FROM [MST].OperationAttribute AS OA
			                    WHERE OA.OperationId=O.Id
			                    GROUP BY OA.UserName
			                    FOR XML PATH ('')
			                    ),1,1,'')
		, OperationAttributeValue=STUFF((SELECT DISTINCT ',' + OAV.UserName FROM [MST].[OperationAttribute] AS OA
			                    LEFT JOIN MST.OperationAttributeValue AS OAV ON OAV.OperationAttributeId=OA.Id
			                    WHERE OA.OperationId=O.Id
			                    GROUP BY OAV.UserName
			                    FOR XML PATH ('')
			                    ),1,1,'')

              , O.CompanyGroupId, O.OperationTypeId, ot.UserName AS OperationTypeCode, O.OperationCategoryId, oc.UserName AS OperationCategoryName
              , O.OperationActivityId, OA.UserName AS OperationActivityName, O.[Sequence], O.Code, O.ShortName
              , O.StandardName, O.UserName, O.Remarks, IsMachineRequired = CASE WHEN O.IsMachineRequired='M' THEN 'YES' ELSE 'NO' END
              , O.BasicProcessTime, O.AssociateProcessTime, O.PersonalAllowance, O.MachineAllowance, O.AdditionalAllowance
	          , ART.MaterialMasterId,MM.Code AS MachineCode,MM.UserName Machine, O.ArticleId, ART.StandardName AS ArticleName, O.SkillId, SK.UserName AS SkillName
	          , O.OperationLength, O.Frequency, O.ProductionSystemId, O.SPI,PS.UserName ProductionSystem
              FROM MST.[Operation] as O
              LEFT JOIN HKP.[OperationType] as ot ON O.OperationTypeId = ot.Id
              LEFT JOIN HKP.[OperationCategory] as oc ON O.OperationCategoryId = oc.Id
              LEFT JOIN HKP.[OperationActivity] AS OA ON O.OperationActivityId=OA.Id
              LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON O.ArticleId=ART.Id
		      LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=ART.MaterialMasterId
              LEFT JOIN [HKP].[Skill] AS SK ON O.SkillId=SK.Id
              LEFT JOIN [HKP].[ProductionSystem] PS ON PS.Id=O.ProductionSystemId
              WHERE O.CompanyGroupId = '" + companyGroupId + "' AND O.Archive = 0";

            return _sqlRepository.GetDataTable(sql);
        }


        private IWorkbook OperationReportList(string companyGroupId, string companyId, string plantId)  //, bool checkbox
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtIssueReportList = GetOperationReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

            if (dtIssueReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "Operation";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            worksheet[ROW, COL].Text = "SL. No";
            int colSLNO = COL;
            worksheet[ROW, COL].ColumnWidth = 5;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Sequence";
            int colSequence = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Code";
            int colCode = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "ShortName";
            int colShortName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Standard Name";
            int colStandardName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "UserName";
            int colUserName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

      
            

            worksheet[ROW, COL].Text = "Operation Activity";
            int colOperationActivityName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Operation Type";
            int colOperationTypeCode = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Operation Category";
            int colOperationCategoryName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Attribute";
            int colOperationAttribute = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Attribute Value";
            int colOperationAttributeValue = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Process";
            int colProcess = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Machine Required";
            int colIsMachineRequired = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Machine Code";
            int colMachineCode = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Machine";
            int colMachine = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticleName = COL;
            worksheet[ROW, COL].ColumnWidth = 28;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Skill";
            int colSkillName = COL;
            worksheet[ROW, COL].ColumnWidth = 23;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Basic Process Time";
            int colBasicProcessTime = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Associate Process Time";
            int colAssociateProcessTime = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Personal Allowance";
            int colPersonalAllowance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Machine Allowance";
            int colMachineAllowance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Additional Allowance";
            int colAdditionalAllowance = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Operation Length";
            int colOperationLength = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "SPI";
            int colSPI = COL;
            worksheet[ROW, COL].ColumnWidth = 7;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Frequency";
            int colFrequency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Production System";
            int colProductionSystem = COL;
            worksheet[ROW, COL].ColumnWidth = 18;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "FGComponent";
            int colFGComponent = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
             COL++;


            worksheet[ROW, COL].Text = "Remarks";
            int colRemarks = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
           // COL++;

            //int colTaskDetail = 0;
            //if (checkbox == true)
            //{
            //    COL++;
            //    colTaskDetail = COL;

            //    worksheet[ROW, COL].Text = "Sub Task";
            //    worksheet[ROW, COL].ColumnWidth = 40;
            //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //}
            //COL++;

            //worksheet[ROW, COL].Text = "SubTaskStatus";
            //int colSubTaskStatus  = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////COL++;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtIssueReportList.Rows.Count; i++)
            {
                worksheet[ROW, colSLNO].Number = (i + 1);

                worksheet[ROW, colSequence].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Sequence"].ToString());
                worksheet[ROW, colSequence].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colCode].Text = dtIssueReportList.Rows[i]["Code"].ToString();
                worksheet[ROW, colShortName].Text = dtIssueReportList.Rows[i]["ShortName"].ToString();
                worksheet[ROW, colStandardName].Text = dtIssueReportList.Rows[i]["StandardName"].ToString();

                
                worksheet[ROW, colFrequency].Number =clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Frequency"].ToString());
                worksheet[ROW, colFrequency].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colUserName].Text = dtIssueReportList.Rows[i]["UserName"].ToString();
                worksheet[ROW, colOperationActivityName].Text = dtIssueReportList.Rows[i]["OperationActivityName"].ToString();
                worksheet[ROW, colOperationTypeCode].Text = dtIssueReportList.Rows[i]["OperationTypeCode"].ToString();

                worksheet[ROW, colOperationCategoryName].Text = dtIssueReportList.Rows[i]["OperationCategoryName"].ToString();
                worksheet[ROW, colOperationAttribute].Text = dtIssueReportList.Rows[i]["OperationAttribute"].ToString();
                worksheet[ROW, colOperationAttributeValue].Text = dtIssueReportList.Rows[i]["OperationAttributeValue"].ToString();

                worksheet[ROW, colProcess].Text = dtIssueReportList.Rows[i]["Process"].ToString();
                worksheet[ROW, colIsMachineRequired].Text = dtIssueReportList.Rows[i]["IsMachineRequired"].ToString();

                worksheet[ROW, colMachineCode].Text = dtIssueReportList.Rows[i]["MachineCode"].ToString();
                //worksheet[ROW, colMachineCode].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colMachine].Text = dtIssueReportList.Rows[i]["Machine"].ToString();


                worksheet[ROW, colArticleName].Text = dtIssueReportList.Rows[i]["ArticleName"].ToString();
                worksheet[ROW, colSkillName].Text = dtIssueReportList.Rows[i]["SkillName"].ToString();

                worksheet[ROW, colFGComponent].Text = dtIssueReportList.Rows[i]["FGComponent"].ToString();


                worksheet[ROW, colBasicProcessTime].Number =clsStaticInfo.dbl(dtIssueReportList.Rows[i]["BasicProcessTime"].ToString());
                worksheet[ROW, colBasicProcessTime].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colAssociateProcessTime].Number =clsStaticInfo.dbl(dtIssueReportList.Rows[i]["AssociateProcessTime"].ToString());
                worksheet[ROW, colAssociateProcessTime].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colPersonalAllowance].Number =clsStaticInfo.dbl(dtIssueReportList.Rows[i]["PersonalAllowance"].ToString());
                worksheet[ROW, colPersonalAllowance].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colMachineAllowance].Number =clsStaticInfo.dbl(dtIssueReportList.Rows[i]["MachineAllowance"].ToString());
                worksheet[ROW, colMachineAllowance].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colAdditionalAllowance].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["AdditionalAllowance"].ToString());
                worksheet[ROW, colAdditionalAllowance].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colOperationLength].Number = clsStaticInfo.dbl (dtIssueReportList.Rows[i]["OperationLength"].ToString());
                worksheet[ROW, colOperationLength].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colSPI].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["SPI"].ToString());
                worksheet[ROW, colSPI].NumberFormat = clsStaticInfo.NumberFormat(0);

                worksheet[ROW, colProductionSystem].Text = dtIssueReportList.Rows[i]["ProductionSystem"].ToString();
                worksheet[ROW, colRemarks].Text = dtIssueReportList.Rows[i]["Remarks"].ToString();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, " Operation Report", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }
        #endregion 
    }
}