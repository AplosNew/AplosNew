using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
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

namespace Aplos.Areas.Employees.Controllers
{
    public class SkillMatrixController : BaseController
    {
        // GET: Employees/SkillMatrix
        #region Constructor

        private readonly ISkillMatrixService _skillMatrixService;
        //private readonly IPurchaseOrderDetailService _inventoryDetailService;
        //private readonly IInventoryMaterialService _inventoryMaterialService;
        //private readonly IInventoryServiceService _inventoryService;
        // private readonly IInventoryReceiveReportService _inventoryReportService;
        //  private readonly DBService _dbService;
        private readonly ISqlRepository _sqlRepository;

        public SkillMatrixController(ISkillMatrixService skillMatrixService
            , ISqlRepository sqlRepository)
        {
            _skillMatrixService = skillMatrixService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetSkillMaster()
        {
            var res = _skillMatrixService.GetSkillMaster();

            //var x= Json(_skillMatrixService.GetSkillMaster(), JsonRequestBehavior.AllowGet);
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //Session["res"] = res.Select(p => new { p.sk, p.Title });
            //foreach (var item in res)
            //{
            //    item.
            //}
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        //[Authorize, HttpGet]
        //[System.Web.Http.FromBody]string queryString, [System.Web.Http.FromBody]string queryStringCaption, [System.Web.Http.FromBody]string queryStringProcess, [System.Web.Http.FromBody]string queryStringSkill, [System.Web.Http.FromBody]string queryStringGrouping, [System.Web.Http.FromBody]string queryStringMachineCategory, [System.Web.Http.FromBody]string queryStringMachineSubCategoryCode, [System.Web.Http.FromBody]string queryStringOnRoll, [System.Web.Http.FromBody]string queryStringTotalPresent, [System.Web.Http.FromBody]string queryStringOnRollShort, [System.Web.Http.FromBody]string queryStringOnRollExcess, [System.Web.Http.FromBody]string queryStringPresentShort, [System.Web.Http.FromBody]string queryStringPresentExcess
        [Authorize, HttpPost]
        public JsonResult GetSkillMasterDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return null;
            var jsondata = Json(_skillMatrixService.GetSkillMasterDetail(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [Authorize, HttpPost]
        public JsonResult GetGraphDetails(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillMatrixService.GetGraphDetails(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetGraphDetails1()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_skillMatrixService.GetGraphDetails1(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProcess()
        {
            return Json(new SelectList(_skillMatrixService.GetProcess(), "drpValue", "drpText"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetEntity()
        {
            return Json(new SelectList(_skillMatrixService.GetEntity(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetEntiryWiseData(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            return Json(_skillMatrixService.GetEntiryWiseData(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetSkillMasterDetailsSummary(string queryString, string queryStringProcess, string queryStringSkill, string queryStringOperationCode, string queryStringGrouping, string queryStringMachineCategory, string queryStringMachineSubCategoryCode, string queryStringCaption, string queryStringOperationCategoryId, string queryStringOnRoll, string queryStringTotalPresent, string queryStringOnRollShort, string queryStringOnRollExcess, string queryStringPresentShort, string queryStringPresentExcess)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return null;
            var jsondata = Json(_skillMatrixService.GetSkillMasterDetailSummary(queryString, queryStringProcess, queryStringSkill, queryStringOperationCode, queryStringGrouping, queryStringMachineCategory, queryStringMachineSubCategoryCode, queryStringCaption, queryStringOperationCategoryId, queryStringOnRoll, queryStringTotalPresent, queryStringOnRollShort, queryStringOnRollExcess, queryStringPresentShort, queryStringPresentExcess), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [Authorize, HttpPost]
        public JsonResult Designation()
        {

            var jsondata = Json(_skillMatrixService.Designation());
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize, HttpGet]
        public ActionResult MatrixReport(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string MaterialId, string ArticleId, string queryString)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Operation Activity Wise Employee Report";
            var workbook = _skillMatrixService.MatrixReport(identity.CompanyId, plantId, fromDate, toDate, Qty, Amount, RcptIssue, MaterialId, ArticleId, queryString);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetOperationWiseInformation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            #region Variable

            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsOperationName = null;
            DataTable dtEntity = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                ru = new ReportUtility();

                objRpt = new clsReport(_sqlRepository);

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";

                para.PlantId = identity.PlantId;


                #endregion Variable

                #region DataSet

                objRpt.GetOperationListSql(out dsOperationName);

                //Dictionary<string, List<DataRow>> dicBonus = objRpt.GetMonthWiseEmpBonusInfo("", "", identity.CompanyGroupId, identity.CompanyId, identity.PlantId, dsOperationName.Tables[0]);
                //dtEntity = GetEntityInfo/*D*/atatable();



                DataTable dtOperationName = dsOperationName.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                var colSr = 0;
                var colCategory = 0;
                var ColEntityName = 0;
                var colEmpName = 0;
                var colTotalAmount = 0;
                var colBonusPercentage = 0;
                var colBonusAmount = 0;
                var colDOS = 0;
                var colWageLabel = 0;

                #endregion------------------Column Header------------------


                var oRU = new ReportUtility();


                var _total_head_count = 0;
                List<OperationSequence> list = null;

                SetHeaderValue("Entity", sheet1, xlsRow, ref xlsCol, out colSr, 25);
                SetHeaderValue("Category", sheet1, xlsRow, ref xlsCol, out colCategory, 25);
                //SetHeaderValue("EntityName", sheet1, xlsRow, ref xlsCol, out ColEntityName, 11);

                var colStart = colCategory;
                CreateDynamicOperationHead(dtOperationName, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref colStart, out list);
                xlsCol--;

                //Dictionary<string, List<DataRow>> dicEntityWiseOperation = GetOperationWiseDataInfo( identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                DataTable dtOperationCategory = GetOperationWiseDataInfoDatatable(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
                string[] arrayList;
                string[] arrayListColumn;
                var dvOnRoleEmp = new DataView(dtOperationCategory);
                 dtEntity = dvOnRoleEmp.ToTable(true, "EntityId", "EntityName");


                // Initialization of array
                arrayList = new string[5] { "Budget", "On Roll", "On Roll - Short / Excess", "Present", "Present - Short / Excess" };
                arrayListColumn = new string[5] { "ManpowerBudget", "OnRoll", "OnRollShortExcess", "TotalPresent", "TotalPresentShortExcess"};

                //report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "EntityName");
                //var colEntityName = sheet1headreColIndex;
                //sheet1headreColIndex++;
               

                endXlsCol = xlsCol;
                var fPanRow = xlsRow + 1;



                xlsRow++;
                int firstEntityRow = 0;

                for (int ie = 0; ie < dtEntity.Rows.Count; ie++)
                {
                    firstEntityRow = xlsRow;
                    string ent = dtEntity.Rows[ie]["EntityName"].ToString();
                    sheet1.Range[xlsRow, 1].Text = ent;


                    for (int i = 0; i < arrayList.Length; i++)
                    {
                        string _categoryDis = arrayList[i];
                        string _category = arrayListColumn[i];
                        sheet1.Range[xlsRow, colCategory].Text = _categoryDis;
                        sheet1.Range[xlsRow, colCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, colCategory].BorderAround(ExcelLineStyle.Hair);


                        for (int c = 0; c < list.Count(); c++)
                        {
                            var v = list[c];
                            DataView dvV = new DataView(dtOperationCategory);
                            //dvV.RowFilter = "EntityName='" + ent + "'";
                            dvV.RowFilter = "EntityName='" + ent + "' and OperationActivity='" + v.Name + "'";
                            if (dvV.Count > 0)
                            {
                                string onRoll = dvV[0][_category].ToString();
                                //string Op_Name = dvV[0]["OperationActivity"].ToString();

                                sheet1.Range[xlsRow, v.XLColIndex].Number = clsStaticInfo.dbl(onRoll);
                                sheet1.Range[xlsRow, v.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, v.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                            }
                            sheet1.Range[xlsRow, v.XLColIndex].BorderAround(ExcelLineStyle.Hair);

                        }

                        xlsRow++;
                    }
                        sheet1.Range[firstEntityRow, 1, xlsRow -1, endXlsCol].BorderAround(ExcelLineStyle.Thick);


                }
                xlsRow++;
                //DataSet dsMain = null;
                //for (int i = 0; i < dtOperationCategory.Rows.Count; i++)
                //{
           

                //    string onRoll = dtOperationCategory.Rows[i]["OnRoll"].ToString();
                //    string Op_Name = dtOperationCategory.Rows[i]["OperationActivity"].ToString();
                //    for (int c = 0; c < list.Count(); c++)
                //    {
                //        var v = list[c];
                //        if (v.Name == Op_Name)
                //        {
                //            sheet1.Range[xlsRow, v.XLColIndex].Text = onRoll;
                //            sheet1.Range[xlsRow, v.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //            sheet1.Range[xlsRow, v.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //            break;
                //        }
                //    }
                //    xlsRow++;
                //}


                
                #region ******************Report Header******************
                //DataView view = new DataView(dicBonus.Values.ElementAt(0)[0].Table);
                //DataTable dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "BankName", "BankShortName", "BankAccNo", "DOS", "PaymentMode");

                xlsRow++;

                //for (int i = 0; i < arrayList.Length; i++)
                //{
                //    sheet1.Range[xlsRow, colCategory].Text = arrayList[i];
                //    sheet1.Range[xlsRow, colCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    sheet1.Range[xlsRow, colCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //    sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                //    sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                //}

                //sheet1.Range[fPanRow, colCategory + 1, xlsRow -1, endXlsCol].BorderInside(ExcelLineStyle.Hair);




                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                xlsRow = 1;
                xlsCol = 1;
                FactoryName = string.Empty;
                var FactoryAddress = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Operation Activity wise Employee ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.UsedRange["A" + fPanRow].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;

                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;

                sheet1.Name = "OperationWiseReport" + para.SalaryProcessId;
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "OperationRegister.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

                //}
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }
        [HttpPost, Authorize]
        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        [HttpPost, Authorize]
        private void CreateDynamicOperationHead(DataTable dtOperationList, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColStart, out List<OperationSequence> list)
        {
            try
            {
                list = new List<OperationSequence>();
                _total_head_count = 0;

                int countGross = 0;
         
                for (int ci = 0; ci < dtOperationList.Rows.Count; ci++)
                {
                    _total_head_count++;
                    countGross++;
                    sheet1.Range[xlsRow, ColStart + countGross].Text = dtOperationList.Rows[ci]["OperationActivity"].ToString();
                    sheet1.Range[xlsRow, ColStart + countGross].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColStart + countGross].CellStyle.Font.Bold = true;
                    //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet1.Range[xlsRow, ColStart + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].BorderAround(ExcelLineStyle.Thin);

                    OperationSequence operationSequence = new OperationSequence();
                    operationSequence.Name = dtOperationList.Rows[ci]["OperationActivity"].ToString();
                    operationSequence.Code = dtOperationList.Rows[ci]["OperationActivityCode"].ToString();
                   
                    operationSequence.XLColIndex = ColStart + countGross;

                    list.Add(operationSequence);
                    xlsCol += 1;
                }//for         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public Dictionary<string, List<DataRow>> GetOperationWiseDataInfo(string companyGroupId, string companyId, string plantId)
        {
            string strSql = @"SELECT --OperationId
	                        EntityId
							,EntityName
							 , OperationCode
	                        ,OperationName
							,OperationActivity
	                        --,OperationCategoryId
	                        --,OperationCategoryName
	                        --,MachineMasterId
	                        --,MachineMaster MachineMaster
	                        --,MachineCategoryId
	                       -- ,MachineCategory
	                        --,MachineSubCategoryId
	                        --,MachineSubCategory
	                       -- ,SkillId
	                       -- ,Type
	                       -- ,Skill
	                        --,SkillGroupId
	                        --,SkillGroupe
	                        --,Position
	                        --,EntityId
	                        --,EntityName
	                        --,ProcessId
	                        --,Process Process
	                        ,Sum(StandardSalary) StandardSalary
	                        ,Sum(ManpowerBudget) ManpowerBudget
	                        ,Sum(OnRoll) OnRoll
	                        ,Sum(OnRollShort) OnRollShort
	                        ,Sum(OnRollExcess) OnRollExcess
	                        ,Sum(TotalPresent) TotalPresent
	                        ,Sum(PresentShort) PresentShort
	                        ,Sum(PresentExcess) PresentExcess
                        FROM (
	                       SELECT OperationActivity , OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
							EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
							FROM (
								SELECT OA.UserName OperationActivity ,OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName, OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
									OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, 'Blank') EntityId, ISNULL(Entity.UserName, 'Blank') EntityName, 
									ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
													OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
								FROM MST.OperationMaster
								LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
								LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
								LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
								LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
								LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
								LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
								LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
								LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
								LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
								LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
								LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
								LEFT OUTER JOIN [HKP].[OperationActivity] OA ON OA.Id=OperationMaster.OperationActivityId
								LEFT OUTER JOIN (
									Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
									) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
								LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									where EmployeeInformation.EmployeeStatus='Active'
									GROUP BY ManpowerBudget.EntityId,OperationMasterId
									) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
								LEFT OUTER JOIN (
									SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
									FROM EmployeeInformation
									LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
									LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
									WHERE AttdnProcessData.DayStatus IN (
											SELECT DayType
											FROM DayType
											WHERE Category = 'Present'
												OR Category = 'Late'
											)
										AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
									GROUP BY ManpowerBudget.EntityId, OperationMasterId
									) Present ON OperationManpowerBudget.EntityId = Present.EntityId
									AND OperationMaster.Id = Present.OperationMasterId
								) Main 
								
	                        ) xyz					
							
                        where isnull(EntityId,'') in('','4')

						GROUP BY 
						OperationName 
						,OperationCode
						,EntityName
						,EntityId
						,OperationActivity 
					Order By EntityName,OActivitySequence";
            DataTable dt = _sqlRepository.GetDataTable(strSql);

            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            List<DataRow> _data = new List<DataRow>();
            string OperationCode = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (OperationCode != dt.Rows[i]["OperationCode"].ToString())
                {
                    _data = new List<DataRow>();
                    dicBonus.Add(dt.Rows[i]["OperationCode"].ToString(), _data);
                }
                _data.Add(dt.Rows[i]);

                OperationCode = dt.Rows[i]["OperationCode"].ToString();
            }

            return dicBonus;
        }
        [HttpPost, Authorize]
        public DataTable GetOperationWiseDataInfoDatatable(string companyGroupId, string companyId, string plantId)
        {
            try
            {
                //string paramters = "";
                string strSql = @"SELECT --OperationId
	                                    EntityId
		                                ,EntityName
		                                -- , OperationCode
	                                    --,OperationName
		                                ,OperationActivity
                                        ,OActivitySequence
	    
	                                    ,Sum(StandardSalary) StandardSalary
	                                    ,Sum(ManpowerBudget) ManpowerBudget
	                                    ,Sum(OnRoll) OnRoll
		                                ,Sum(OnRoll) -Sum(ManpowerBudget)  OnRollShortExcess
	                                    ,Sum(OnRollShort) OnRollShort
	                                    ,Sum(OnRollExcess) OnRollExcess
		                                ,Sum(TotalPresent) TotalPresent
	                                    ,Sum(OnRoll) -Sum(TotalPresent)  TotalPresentShortExcess
	                                    ,Sum(PresentShort) PresentShort
	                                    ,Sum(PresentExcess) PresentExcess
                                    FROM (
	                                    SELECT OperationActivity ,OActivitySequence, OperationId, OperationCode, OperationName, OperationCategoryId, OperationCategoryName, MachineMasterId, MachineMasterName MachineMaster, MachineCategoryId, MachineCategory, MachineSubCategoryId, MachineSubCategory, SkillId, Type, Skill, SkillGroupId, SkillGroupe, 
		                                EntityId, EntityName, ProcessId, ProcessName Process, ManpowerBudget, StandardSalary, OnRoll, OnRollShort, OnRollExcess, TotalPresent, PresentShort, PresentExcess
		                                FROM (
			                                SELECT OA.UserName OperationActivity , OA.Sequence OActivitySequence,OperationMaster.Id OperationId, OperationMaster.Code OperationCode, OperationMaster.UserName OperationName, OperationMaster.OperationActivityId, OperationMaster.Type, OperationActivity.UserName OperationActivityName, OperationMaster.OperationTypeId, OperationType.UserName OperationTypeName, OperationMaster.OperationCategoryId, OperationCategory.UserName OperationCategoryName, OperationMaster.Type OperationOrActivity, ISNULL(OperationMaster.MachineMasterId, 'N/A') MachineMasterId, ISNULL(MachineMaster.UserName, 'N/A') MachineMasterName, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineCategory, ISNULL(MachineMaster.MachineSubCategoryId, 'N/A') MachineSubCategoryId, ISNULL(MachineSubCategory.UserName, 'N/A') MachineSubCategory, SkillId = CASE WHEN OperationMaster.Type = 'Activity' THEN OperationMaster.SkillId ELSE MachineMaster.SkillId END, Skill = CASE WHEN OperationMaster.Type = 'Activity' THEN Skill.UserName ELSE MachineSkill.UserName END,
				                                OperationMaster.SkillGroupId, SkillGrouping.UserName SkillGroupe, SkillGrouping.StandardSalary, OperationMaster.LegalDesignationId, LegalDesignation.UserName LegalDesignationName, OperationMaster.ProcessId, Process.UserName ProcessName, OperationMaster.ProposedSalary, IsNull(OperationManpowerBudget.EntityId, '') EntityId, ISNULL(Entity.UserName,  '') EntityName, 
				                                ISNULL(OperationManpowerBudget.ManpowerBudget, 0) ManpowerBudget, ISNULL(OnRoll.OnRollManpower, 0) OnRoll, ISNULL(Present.DayPresentCount, 0) TotalPresent, OnRollShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0)) ELSE 0 END, OnRollExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(
								                                OnRoll.OnRollManpower, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(OnRoll.OnRollManpower, 0))) ELSE 0 END, PresentShort = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) > 0 THEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) ELSE 0 END, PresentExcess = CASE WHEN CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0)) < 0 THEN ABS(CONVERT(NUMERIC(10, 0), OperationManpowerBudget.ManpowerBudget - ISNULL(Present.DayPresentCount, 0))) ELSE 0 END
			                                FROM MST.OperationMaster
			                                LEFT OUTER JOIN HKP.OperationActivity ON OperationMaster.OperationActivityId = OperationActivity.Id
			                                LEFT OUTER JOIN HKP.OperationType ON OperationMaster.OperationTypeId = OperationType.Id
			                                LEFT OUTER JOIN HKP.OperationCategory ON OperationMaster.OperationCategoryId = OperationCategory.Id
			                                LEFT OUTER JOIN MST.MachineMaster ON OperationMaster.MachineMasterId = MachineMaster.Id
			                                LEFT OUTER JOIN HKP.MachineCategory ON MachineMaster.MachineCategoryId = MachineCategory.Id
			                                LEFT OUTER JOIN HKP.MachineSubCategory ON MachineMaster.MachineSubCategoryId = MachineSubCategory.Id
			                                LEFT OUTER JOIN HKP.Skill ON OperationMaster.SkillId = Skill.Id
			                                LEFT OUTER JOIN SCS.SkillGrouping ON OperationMaster.SkillGroupId = SkillGrouping.Id
			                                LEFT OUTER JOIN HKP.LegalDesignation ON OperationMaster.LegalDesignationId = LegalDesignation.Id
			                                LEFT OUTER JOIN HKP.Process ON OperationMaster.ProcessId = Process.Id
			                                LEFT OUTER JOIN HKP.Skill MachineSkill ON MachineMaster.SkillId = MachineSkill.Id
			                                LEFT OUTER JOIN [HKP].[OperationActivity] OA ON OA.Id=OperationMaster.OperationActivityId
			                                LEFT OUTER JOIN (
				                                Select CompanyGroupId,EntityId,OperationMasterId,sum(ManpowerBudget) ManpowerBudget from mst.OperationPositionMPBudget group by CompanyGroupId,EntityId,OperationMasterId
				                                ) OperationManpowerBudget on OperationManpowerBudget.OperationMasterId = OperationMaster.Id and OperationManpowerBudget.CompanyGroupId = OperationMaster.CompanyGroupId
			                                LEFT OUTER JOIN ORG.Entity ON OperationManpowerBudget.EntityId = Entity.Id	
			                                LEFT OUTER JOIN (
				                                SELECT ManpowerBudget.EntityId,ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) OnRollManpower
				                                FROM EmployeeInformation
				                                LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
				                                where EmployeeInformation.EmployeeStatus='Active'
				                                GROUP BY ManpowerBudget.EntityId,OperationMasterId
				                                ) OnRoll ON OperationManpowerBudget.EntityId = OnRoll.EntityId AND OperationMaster.Id = OnRoll.OperationMasterId
			                                LEFT OUTER JOIN (
				                                SELECT ManpowerBudget.EntityId, ISNULL(OperationMasterId, '') OperationMasterId, Count(EmployeeInformation.SystemId) DayPresentCount
				                                FROM EmployeeInformation
				                                LEFT OUTER JOIN MST.ManpowerBudget ON ManpowerBudget.Id = EmployeeInformation.BudgetCode
				                                LEFT OUTER JOIN AttdnProcessData ON EmployeeInformation.SystemId = AttdnProcessData.EmpSystemID
				                                WHERE AttdnProcessData.DayStatus IN (
						                                SELECT DayType
						                                FROM DayType
						                                WHERE Category = 'Present'
							                                OR Category = 'Late'
						                                )
					                                AND AttdnProcessData.WorkDate = REPLACE(Convert(VARCHAR(11), getdate(), 106), ' ', '-')
				                                GROUP BY ManpowerBudget.EntityId, OperationMasterId
				                                ) Present ON OperationManpowerBudget.EntityId = Present.EntityId
				                                AND OperationMaster.Id = Present.OperationMasterId
			                                ) Main 
								
	                                    ) xyz					
							
                                    where EntityName !=''

	                                GROUP BY 
	                                --OperationName 
	                                --,OperationCode
	                                EntityName
	                                ,EntityId,OActivitySequence
	                                ,OperationActivity 
	                                Order By EntityName,OActivitySequence";
                DataTable dt = _sqlRepository.GetDataTable(strSql);
                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost, Authorize]

        public DataTable GetEntityInfoDatatable()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //string paramters = "";
                string strSql = @"select * from ORG.Entity where Active = 1 and PlantId = '"+ identity.PlantId+ @"'";
                DataTable dt = _sqlRepository.GetDataTable(strSql);
                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost, Authorize]
        string GetDecimalFormat(bool isInt, int decimalNo)
        {
            try
            {
                var ob = new ReportUtility();
                if (isInt == true)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(decimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}