#region Using

using Library.Core;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using System.Web.Mvc;
using System.Threading;
using Library.Crosscutting.Security;
using System.Collections.Generic;
using Library.Data.Sql;
using Syncfusion.XlsIO;
using Library.Data;
using System.Data;
using System;
using OTSBD;
using Library.Service.Helpers;

#endregion Using

namespace Aplos.Areas.Machines.Controllers
{
    public class OperationVariationController : Controller
    {
        #region -- Constrator

        private readonly IOperationVariationService _operationVariationService;
        private readonly ISqlRepository _sqlRepository;

        public OperationVariationController(IOperationVariationService operationVariationService, ISqlRepository sqlRepository)
        {
            _operationVariationService = operationVariationService;
            _sqlRepository = sqlRepository;
        }

        #endregion -- Constrator

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- OperationVariation
        [HttpPost, Authorize]
        public ActionResult GetProductMasterList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT Flag=CAST(0 as bit),PM.Id ProductMasterId,PM.Sequence,PM.Code,PM.ShortName,PM.StandardName,PM.UserName, PC.UserName AS ProductCategoryName, PSC.UserName AS ProductSubCategoryName, P.UserName AS ProductName, PR.UserName BaseProcess, UOMB.UserName AS BaseUom,PM.Active 
                                        FROM MST.[ProductMaster] AS PM 
                                        LEFT OUTER JOIN HKP.[ProductCategory] AS PC ON PC.Id = PM.ProductCategoryId
                                        LEFT OUTER JOIN HKP.[ProductSubCategory] AS PSC ON PSC.Id = PM.ProductSubCategoryId
                                        LEFT OUTER JOIN HKP.[Product] AS P ON P.Id = PM.ProductId
                                        LEFT OUTER JOIN HKP.[Process] AS PR ON PR.Id = PM.BaseProcessId
                                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UOMB ON PM.BaseUOMId = UOMB.Id
                                        WHERE PM.CompanyGroupId = '" + identity.CompanyGroupId + "' AND PM.Archive = 0) AS TEMP WHERE " + strkey + " order by sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetOperationVariationSizeGroup(string operationVariationId)
        {
            string sql = @"SELECT O.*,S.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName FROM dbo.OperationVariationSizeGroup O
                            LEFT JOIN HKP.SizeGroup S ON  S.Id=O.SizeGroupId
                            WHERE O.OperationVariationId='"+ operationVariationId + "' ORDER BY S.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationVariationPM(string operationVariationId)
        {
            string sql = @"SELECT OP.*,PM.Sequence,PM.Code,PM.ShortName,PM.StandardName,PM.UserName, PC.UserName AS ProductCategoryName, PSC.UserName AS ProductSubCategoryName, P.UserName AS ProductName, PR.UserName BaseProcess, UOMB.UserName AS BaseUom,PM.Active 
                                        FROM dbo.OperationVariationProductMaster OP
										LEFT JOIN MST.[ProductMaster] AS PM ON PM.Id=OP.ProductMasterId
                                        LEFT OUTER JOIN HKP.[ProductCategory] AS PC ON PC.Id = PM.ProductCategoryId
                                        LEFT OUTER JOIN HKP.[ProductSubCategory] AS PSC ON PSC.Id = PM.ProductSubCategoryId
                                        LEFT OUTER JOIN HKP.[Product] AS P ON P.Id = PM.ProductId
                                        LEFT OUTER JOIN HKP.[Process] AS PR ON PR.Id = PM.BaseProcessId
                                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UOMB ON PM.BaseUOMId = UOMB.Id
                            WHERE OP.OperationVariationId='" + operationVariationId + "' ORDER BY PM.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetOperationData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT O.Id
	                    , Process=STUFF((SELECT DISTINCT ',' + P.UserName FROM [MST].[OperationProcess] AS OPMT
					                    LEFT JOIN HKP.[Process] AS P ON OPMT.ProcessId=P.Id
					                    WHERE OPMT.OperationId=O.Id
					                    GROUP BY P.UserName
					                    FOR XML PATH ('')
					                    ),1,1,'')
, ProsessIds=(SELECT STUFF((SELECT DISTINCT ',' +  ProcessId FROM [MST].[OperationProcess] WHERE OperationId=O.Id FOR XML PATH('')),1,1,''))
                        , O.CompanyGroupId, O.OperationTypeId, ot.UserName AS OperationTypeCode, O.OperationCategoryId, oc.UserName AS OperationCategoryName
                        , O.OperationActivityId, OA.UserName AS OperationActivityName, O.[Sequence], O.Code, O.ShortName
                        , O.StandardName, O.UserName, O.Remarks, IsMachineRequired = CASE WHEN O.IsMachineRequired='M' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, O.Active, O.Archive                       
                        , O.BasicProcessTime, O.AssociateProcessTime, O.PersonalAllowance, O.MachineAllowance
	                    , ART.MaterialMasterId, O.ArticleId, ART.StandardName AS ArticleName, O.SkillId, SK.UserName AS SkillName
	                    , O.OperationLength, O.Frequency, O.ProductionSystemId, O.SPI,O.AdditionalAllowance,OV.UserName OperationVariation
                        FROM MST.[Operation] as O 				
					LEFT JOIN [MST].[OperationVariation] OV	 ON OV.OperationId=O.Id
                    LEFT JOIN HKP.[OperationType] as ot ON O.OperationTypeId = ot.Id
                    LEFT JOIN HKP.[OperationCategory] as oc ON O.OperationCategoryId = oc.Id
                    LEFT JOIN HKP.[OperationActivity] AS OA ON O.OperationActivityId=OA.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON O.ArticleId=ART.Id
                    LEFT JOIN [HKP].[Skill] AS SK ON O.SkillId=SK.Id
                    WHERE O.CompanyGroupId = '" + identity.CompanyGroupId + "' AND O.Archive = 0 Order by O.UserName";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo(string operationId)
        {
            return Json(_operationVariationService.GetCbo(operationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string operationId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationVariationService.Query(parameters, identity.CompanyGroupId, operationId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetVairiationValue(string operationId, string masterId)
        {
            return Json(_operationVariationService.GetVairiationValue(operationId, masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUtilityByOperationData(string operationId, string articleId, string skillId)
        {
            return Json(_operationVariationService.GetOperationUtilityData(operationId, articleId, skillId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetMachineListByOperation(GridParameter parameters, string operationId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationVariationService.GetMachineListByOperation(parameters, identity.CompanyGroupId, operationId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_operationVariationService.GetAutoSequence(identity.CompanyGroupId, id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(OperationVariation operationVariation, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            operationVariation.CompanyGroupId = identity.CompanyGroupId;
            _operationVariationService.InsertGraph(operationVariation, valueList, operationVariationSizeGroupDataList, operationVariationPMDataList);
            return Json(new { OperationVariation = operationVariation, Sequence = _operationVariationService.GetAutoSequence(operationVariation.CompanyGroupId, operationVariation.OperationId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(OperationVariation operationVariation, IEnumerable<OperationVariationAttributeValue> valueList, IEnumerable<OperationVariationSizeGroup> operationVariationSizeGroupDataList, IEnumerable<OperationVariationProductMaster> operationVariationPMDataList)
        {
            _operationVariationService.UpdateGraph(operationVariation, valueList, operationVariationSizeGroupDataList, operationVariationPMDataList);
            return Json(new { Sequence = _operationVariationService.GetAutoSequence(operationVariation.CompanyGroupId, operationVariation.OperationId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id, string operationId)
        {
            var entity = _operationVariationService.Find(id);
            _operationVariationService.DeleteGraph(id);
            return Json(new { Sequence = _operationVariationService.GetAutoSequence(entity.CompanyGroupId, operationId), Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult DeleteOperationVariationSizeGroup(string id)
        {
            _operationVariationService.DeleteOperationVariationSizeGroup(id);
            return Json(new {Message = AplosMessage.Deleted });
        }
        #endregion -- OperationVariation


        #region Report
        [HttpGet, Authorize]
        public ActionResult GetOperationVariationReportExcel()   //bool checkbox
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
                IWorkbook workbook = OperationVariationReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                string strFileName = "Operation Variation Report.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        //IssueReport
        private DataTable GetOperationVariationReportData(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"               
          SELECT OS.*, ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime , ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance
                                            , OP.OperationLength, 
                                            -- OP.IsMachineRequired
                                               IsMachineRequired = CASE WHEN OP.IsMachineRequired='M' THEN 'YES' ELSE 'NO' END
                                            , ART.StandardName AS ArticleName, SK.UserName AS SkillName, mm.code as MachineCode, MM.UserName Machine,OM.Code OperationMasterCode,op.UserName Operation 
											
											, SizeGroup=STUFF((SELECT DISTINCT ',' + P.UserName FROM [dbo].[OperationVariationSizeGroup] AS OPMT
														LEFT JOIN HKP.[SizeGroup] AS P ON OPMT.SizeGroupId=P.Id
														WHERE OPMT.OperationVariationId=OS.Id
														GROUP BY P.UserName
														FOR XML PATH ('')
														),1,1,'')
                                          
								, OperationAttributeValue=STUFF((SELECT DISTINCT ',' +A.Value 
								--,op.MachineAllowance
                               
								 FROM (
								 select Id,Value from(
							     SELECT 
								 OVAV.OperationId AS Id,Value=CASE WHEN ISNULL(OVAV.OperationAttributeValueId,'')='' THEN ISNULL(OVAV.AttributeValueFreeText,'') ELSE OAV.UserName END + ' ('+OA.UserName+')'
								 FROM [MST].[OperationAttribute] AS OA
			                     JOIN MST.OperationVariationAttributeValue AS OVAV ON OVAV.OperationAttributeId=OA.Id
								 JOIN MST.OperationAttributeValue AS OAV ON OAV.OperationAttributeId=OA.Id) OA
								 ) A
			                    WHERE A.Id=OP.Id
			                    GROUP BY A.Value
			                    FOR XML PATH ('')
			                    ),1,1,'')
                          		        
                            FROM [MST].[OperationVariation] AS OS
                            JOIN [MST].[Operation] AS OP ON OP.Id = OS.OperationId
                            LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON OS.ArticleId = ART.Id
                            LEFT JOIN [MST].[MaterialMaster] AS MM ON MM.Id=ART.MaterialMasterId
                            LEFT JOIN [MST].[OperationMaster]  OM ON OM.Id=OS.OperationMasterId
                            JOIN [HKP].[Skill] AS SK ON OP.SkillId = SK.Id
                            WHERE OS.CompanyGroupId='" + companyGroupId + "'  order by op.UserName";

            return _sqlRepository.GetDataTable(sql);
        }


        private IWorkbook OperationVariationReportList(string companyGroupId, string companyId, string plantId)  //, bool checkbox
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
            DataTable dtIssueReportList = GetOperationVariationReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

            if (dtIssueReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "Operation Variation";

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
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Short Name";
            int colShortName = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Standard Name";
            int colStandardName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "User Name";
            int colUserName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Operation";
            int colOperation = COL;
            worksheet[ROW, COL].ColumnWidth = 28;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Machine Required";
            int colIsMachineRequired = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Machine Code";
            int colMachineCode = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Machine";
            int colUserNameMachine = COL;
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

            //worksheet[ROW, COL].Text = "Material";
            //int colMaterialName = COL;
            //worksheet[ROW, COL].ColumnWidth = 20;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;


            worksheet[ROW, COL].Text = "Operation Master";
            int colOperationMasterCode = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Size Group";
            int colSizeGroup = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Attribute Value";
            int colOperationAttributeValue = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Basic Process Time";
            int colBasicProcessTime = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Associate Process Time";
            int colAssociateProcessTime = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Personal Allowance";
            int colpersonalallowance = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Machine Allowance";
            int colMachineAllowance = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Additional Allowance";
            int colAdditionalAllowance = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Operation SPT";
            int colSubOperationSAM = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SPI";
            int colSPI = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Frequency";
            int colFrequencye = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Additional SAM Symbol";
            int colAdditionalSAMSymbol = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Additional SPT";
            int colAdditionalSAM = COL;
            worksheet[ROW, COL].ColumnWidth = 13;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Total SPT";
            int colTotalSAM = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;

            worksheet[ROW, COL].Text = "Operation Length";
            int colOperationLength = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            //worksheet[ROW, COL].Text = "Remarks";
            //int colRemarks = COL;
            //worksheet[ROW, COL].ColumnWidth = 20;
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

                worksheet[ROW, colSequence].Number =clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Sequence"].ToString());
                worksheet[ROW, colSequence].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colOperation].Text = dtIssueReportList.Rows[i]["Operation"].ToString();

                worksheet[ROW, colOperationMasterCode].Text = dtIssueReportList.Rows[i]["OperationMasterCode"].ToString();
                worksheet[ROW, colSizeGroup].Text = dtIssueReportList.Rows[i]["SizeGroup"].ToString();
                worksheet[ROW, colOperationAttributeValue].Text = dtIssueReportList.Rows[i]["OperationAttributeValue"].ToString();
                worksheet[ROW, colCode].Text = dtIssueReportList.Rows[i]["Code"].ToString();
                worksheet[ROW, colShortName].Text = dtIssueReportList.Rows[i]["ShortName"].ToString();
                worksheet[ROW, colStandardName].Text = dtIssueReportList.Rows[i]["StandardName"].ToString();
                worksheet[ROW, colUserName].Text = dtIssueReportList.Rows[i]["UserName"].ToString();
          
                worksheet[ROW, colIsMachineRequired].Text = dtIssueReportList.Rows[i]["IsMachineRequired"].ToString();
                worksheet[ROW, colMachineCode].Text =dtIssueReportList.Rows[i]["MachineCode"].ToString();
               // worksheet[ROW, colMachineCode].NumberFormat = clsStaticInfo.NumberFormat(2);


                worksheet[ROW, colBasicProcessTime].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["BasicProcessTime"].ToString());
                worksheet[ROW, colBasicProcessTime].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colAssociateProcessTime].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["AssociateProcessTime"].ToString());
                worksheet[ROW, colAssociateProcessTime].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colpersonalallowance].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["PersonalAllowance"].ToString());
                worksheet[ROW, colpersonalallowance].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colMachineAllowance].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["MachineAllowance"].ToString());
                worksheet[ROW, colMachineAllowance].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colAdditionalAllowance].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["AdditionalAllowance"].ToString());
                worksheet[ROW, colAdditionalAllowance].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colSubOperationSAM].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["SubOperationSAM"].ToString());
                worksheet[ROW, colSubOperationSAM].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colFrequencye].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Frequency"].ToString()); 
                worksheet[ROW, colFrequencye].NumberFormat = clsStaticInfo.NumberFormat(2);

                worksheet[ROW, colSPI].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["SPI"].ToString());
                worksheet[ROW, colSPI].NumberFormat = clsStaticInfo.NumberFormat(0);

                worksheet[ROW, colSubOperationSAM].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["SubOperationSAM"].ToString());
                worksheet[ROW, colSubOperationSAM].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colAdditionalSAMSymbol].Text = dtIssueReportList.Rows[i]["AdditionalSAMSymbol"].ToString();

                worksheet[ROW, colAdditionalSAM].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["AdditionalSAM"].ToString());
                worksheet[ROW, colAdditionalSAM].NumberFormat = clsStaticInfo.NumberFormat(4);

                worksheet[ROW, colTotalSAM].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["TotalSAM"].ToString());
                worksheet[ROW, colTotalSAM].NumberFormat = clsStaticInfo.NumberFormat(4);

                //worksheet[ROW, colRemarks].Text = dtIssueReportList.Rows[i]["Remarks"].ToString();
   


                worksheet[ROW, colUserNameMachine].Text = dtIssueReportList.Rows[i]["Machine"].ToString();

                worksheet[ROW, colArticleName].Text = dtIssueReportList.Rows[i]["ArticleName"].ToString();

                worksheet[ROW, colSkillName].Text = dtIssueReportList.Rows[i]["SkillName"].ToString();

                worksheet[ROW, colOperationLength].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["OperationLength"].ToString());
                worksheet[ROW, colOperationLength].NumberFormat = clsStaticInfo.NumberFormat(2);



                //if (checkbox == true)
                //{

                //    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();

                //}

                // worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colScantionAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["ScantionAmount"].ToString());


                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;



            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, " Operation Variation Report", identity.PlantId);
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