using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Machines.Controllers
{
    public class IncedentUpdateController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public IncedentUpdateController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations


        [Authorize, HttpGet]
        public JsonResult GetIncedentCategoryList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select Id as Value,UserName as Text from [HKP].[IncedentCategory]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetResponsiblePerson()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetROEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as RONameId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as ROName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and MB.ROBudgetCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetFollowUpBy()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as FollowUpById, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as FollowUpBy, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active' and MB.ROBudgetCode is not null";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetIssueInchargeById(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select EI.SystemId as EmpId,EI.EmployeeName from [MST].[ManpowerBudget] MB
                        left join dbo.EmployeeInformation EI On EI.BudgetCode=MB.Id
                        left join [HKP].[IncedentCategory] IC ON IC.InchargeNameBgtCodeId=MB.Id
                        where IC.Id='" + id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIncedentCategoryUpdate()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select *,format(ICU.Date,'dd-MMM-yyyy') as IncedentDate,format(ICU.Time,'hh:mm tt') as IncedentTime,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.EmployeeId) as EmployeeName,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.RONameId) as ROName,
 (select IC.UserName from [HKP].[IncedentCategory] IC where IC.Id=ICU.IncedentCategoryId) as IncedentCategory,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.FollowUpById) as FollowUpBy,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.IssueInchargeId) as IssueIncharge
 from [TRN].[IncedentCategoryUpdate] ICU";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIncedentUpdate(string IncedentId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,format(IU.Date,'dd-MMM-yyyy') as IncedentDate,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=IU.ResponsiblePersonId) as ResponsiblePerson
 from [TRN].[IncedentUpdate] IU where IU.IncedentId='"+ IncedentId + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIncedentUpdateGrid()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select *,format(IU.Date,'dd-MMM-yyyy') as IncedentDate,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=IU.ResponsiblePersonId) as ResponsiblePerson
 from [TRN].[IncedentUpdate] IU";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadICUEditData(string ICUId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,format(ICU.Date,'dd-MMM-yyyy') as IncedentDate,format(ICU.Time,'hh:mm tt') as IncedentTime,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.EmployeeId) as EmployeeName,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.RONameId) as ROName,
 (select IC.UserName from [HKP].[IncedentCategory] IC where IC.Id=ICU.IncedentCategoryId) as IncedentCategory,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.FollowUpById) as FollowUpBy,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.IssueInchargeId) as IssueIncharge
 from [TRN].[IncedentCategoryUpdate] ICU where ICU.Id='" + ICUId + @"'";
            return Json(new { incedentupdate = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadIUEditData(string IUId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select *,format(IU.Date,'dd-MMM-yyyy') as IncedentDate,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=IU.ResponsiblePersonId) as ResponsiblePerson
 from [TRN].[IncedentUpdate] IU where IU.Id='" + IUId + @"'";
            return Json(new { incedentupdates = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(Dictionary<string, object> IncedentUpdateData,string PId)
        {
            try
            {
                ConnectionManager.DAL.ConManager conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[IncedentUpdate] where Id<>'" + IncedentUpdateData["Id"] + "'", out DataSet dsIncedentUpdateValidation, false, "1");

                DataSet dsIncedentUpdate;

                conRack = new ConnectionManager.DAL.ConManager("1");
                conRack.OpenDataSetThroughAdapter("select * from [TRN].[IncedentUpdate] where Id='" + IncedentUpdateData["Id"] + "'", out dsIncedentUpdate, false, "1");
                string _Id = "";

                #region data update
                if (dsIncedentUpdate.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("IncedentUpdate", out _Id);
                    _Id = "IU" + _Id;
                    IncedentUpdateData["Id"] = _Id;
                    IncedentUpdateData["IncedentId"] = PId;
                    AddNewRow(dsIncedentUpdate.Tables[0], IncedentUpdateData);
                }
                else
                {
                    _Id = IncedentUpdateData["Id"].ToString();
                    EditRow(dsIncedentUpdate.Tables[0].Rows[0], IncedentUpdateData);
                }
                #endregion data update



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsIncedentUpdate);

                return Json(new { Error = false, Data = IncedentUpdateData, Message = AplosMessage.Insert });

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


        [Authorize, HttpPost]
        public ActionResult IncedentUpdateDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [TRN].[IncedentUpdate] where Id ='" + id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<System.Web.HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetICUDocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetICUDocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetICUDocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetICUDocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [TRN].[IncedentCategoryUpdate] WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetIncedentReport(string reportFileName)
        {
            try
            {
                string fileName = "";
                fileName = IncedentReportxlx("", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string IncedentReportxlx(string ReportHeader, string reportFileName)
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
                workbook.Worksheets[0].Name = "Incedent Report";
                sheet = workbook.Worksheets[0];
                int ROW = 5; int COL = 1;
                DataTable data = GetIncedentReportData();

                #region columns
                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "RO Name";
                sheet[ROW, COL].ColumnWidth = 20;
                int ColRO = COL;
                COL++;

                sheet[ROW, COL].Text = "Issue AddedBy";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColIAB = COL;
                COL++;

                sheet[ROW, COL].Text = "Added Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColAD = COL;
                COL++;

                sheet[ROW, COL].Text = "UpdatedBy";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColUB = COL;
                COL++;

                sheet[ROW, COL].Text = "Updated Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColUD = COL;
                COL++;

                sheet[ROW, COL].Text = "Incedent Category";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColIC = COL;
                COL++;

                sheet[ROW, COL].Text = "Incedent Item Title";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColIIT = COL;
                COL++;
                sheet[ROW, COL].Text = "Incedent Detail";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColID = COL;
                COL++;
                sheet[ROW, COL].Text = "Incedent Type";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColIT = COL;
                COL++;
                sheet[ROW, COL].Text = "Criticality Level";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColCL = COL;
                COL++;
                sheet[ROW, COL].Text = "Action Taken";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColAT = COL;
                COL++;
                sheet[ROW, COL].Text = "Issue Incharge";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColII = COL;
                COL++;
                sheet[ROW, COL].Text = "FollowUp By";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColFB = COL;
                COL++;
                sheet[ROW, COL].Text = "Final Status";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColFS = COL;

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

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColRO].Text = data.Rows[i]["ROName"].ToString();
                    sheet[ROW, ColIAB].Text = data.Rows[i]["IssueAddedBy"].ToString();
                    sheet[ROW, ColAD].Text = data.Rows[i]["AddedDate"].ToString();
                    sheet[ROW, ColUB].Text = data.Rows[i]["UpdatedBy"].ToString();
                    sheet[ROW, ColUD].Text = data.Rows[i]["UpdatedDate"].ToString();
                    sheet[ROW, ColIC].Text = data.Rows[i]["IncedentCategory"].ToString();
                    sheet[ROW, ColIIT].Text = data.Rows[i]["IncedentItemTitle"].ToString();
                    sheet[ROW, ColID].Text = data.Rows[i]["IncedentDetail"].ToString();
                    sheet[ROW, ColIT].Text = data.Rows[i]["IncedentType"].ToString();
                    sheet[ROW, ColCL].Text = data.Rows[i]["CriticalityLevel"].ToString();
                    sheet[ROW, ColAT].Text = data.Rows[i]["ActionTaken"].ToString();
                    sheet[ROW, ColII].Text = data.Rows[i]["IssueIncharge"].ToString();
                    sheet[ROW, ColFB].Text = data.Rows[i]["FollowUpBy"].ToString();
                    sheet[ROW, ColFS].Text = data.Rows[i]["FinalStatus"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Incedent Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
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

        public DataTable GetIncedentReportData()
        {
            try
            {
                var sql = @"select (select EI.EmployeeCode from EmployeeInformation EI where EI.SystemId=ICU.EmployeeId) as EmployeeCode,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.EmployeeId) as EmployeeName,
 (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.RONameId) as ROName,
 ICU.AddedBy IssueAddedBy,format(ICU.AddedDate,'dd-MMM-yyyy') as AddedDate,ICU.UpdatedBy,format(ICU.UpdatedDate,'dd-MMM-yyyy') as UpdatedDate,
 (select IC.UserName from [HKP].[IncedentCategory] IC where IC.Id=ICU.IncedentCategoryId) as IncedentCategory
 ,ICU.IncedentItemTitle,ICU.IncedentDetail,ICU.IncedentType,ICU.CriticalityLevel,ICU.ActionTaken
, (select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.IssueInchargeId) as IssueIncharge
 ,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=ICU.FollowUpById) as FollowUpBy,ICU.FinalStatus
 from [TRN].[IncedentCategoryUpdate] ICU";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion -- Operations
    }
}