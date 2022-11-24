using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Security.Core;
using Library.Service.Helpers;
using Syncfusion.XlsIO;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ResidenceStatusAllocationController : Controller
    {
        //ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();
        ResidenceStausAllocationService rsl = new ResidenceStausAllocationService();
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;
        public ResidenceStatusAllocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Report()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult getResidenceFilters()
        {
            try
            {
                var sql = @"select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId,EC.UserName EmployeeType
									,EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType,RM.ResidenceNumber,RM.AssetName
									,VacancyStatus = 'Occupied'

									from ResidenceMaster RM
									left join ResidenceGroup RG on RG.Id=RM.ResidenceGroupId 
									left join ORG.Plant P on P.Id=RM.PlantId
									left join HKP.EmployeeCategory EC on EC.Id=RM.EmployeeCategoryId
									left join EmpServiceType EST on EST.Id=RM.EmpServiceTypeId

                union all
                select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId,EC.UserName EmployeeType
									,EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType,RM.ResidenceNumber,RM.AssetName
									,VacancyStatus = 'All'

									from ResidenceMaster RM
									left join ResidenceGroup RG on RG.Id=RM.ResidenceGroupId 
									left join ORG.Plant P on P.Id=RM.PlantId
									left join HKP.EmployeeCategory EC on EC.Id=RM.EmployeeCategoryId
									left join EmpServiceType EST on EST.Id=RM.EmpServiceTypeId";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet, Authorize]
        public JsonResult getemployeeDataList(string plantId,string residenceGroupId, string EmployeeTypeId)
        {
            return Json(rsl.getemployeeDataList(plantId, residenceGroupId, EmployeeTypeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getOccupiedemployeeDataList(string plantId, string residenceNumber)
        {
            return Json(rsl.getOccupiedemployeeDataList(plantId, residenceNumber), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult getResidence()
        {
            return Json(rsl.getResidence(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult viewUnallocation(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(rsl.getviewUnallocation(PlantId), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpPost]
        public ActionResult getAllEmployee(string EmpCategoryId)
        {
            try
            {
                var jsondata = Json(rsl.getAllEmployee(EmpCategoryId), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getEmployeeCategory()
        {
            try
            {
                return Json(rsl.getEmployeeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetViewData(Dictionary<string, string> parameters)
        {
            return Json(rsl.GetViewData(parameters), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult PopupEmployeeView(string fromDate, string toDate, string EmployeeCategorySystemID)
        {
            return Json(rsl.PopupEmployeeView(fromDate, toDate, EmployeeCategorySystemID), JsonRequestBehavior.AllowGet);
        }

        #region Save Operations
        [HttpPost]
        public JsonResult residenceStatusSave(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                rsl.Save(EmployeeList);
                return Json(new { Data = EmployeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult SaveRSUnallocation(List<Dictionary<string, object>> employeeList)
        {

            try
            {
               
                rsl.SaveRSUnallocation(employeeList);
                return Json(new { Data = employeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult getEmployee(string PlantId, string ResidenceGroupId, string EmployeeCategoryId)
        {
            try
            {
                return Json(rsl.getEmployee(PlantId, ResidenceGroupId, EmployeeCategoryId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       
        [HttpPost]
        public ActionResult getResidenceStatusLocation(string EmployeeId, string ResidenceMasterId)
        {
            try
            {
                return Json(rsl.getResidenceStatusLocation(EmployeeId, ResidenceMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /* [HttpPost]
         public JsonResult Delete(string id)
         {
             try
             {
                 rsl.delete(id);
                 return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
             }
             catch (Exception ex)
             {
                 return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
             }

         }*/

        #endregion Save Operations

        [HttpGet, Authorize]
        public ActionResult getResidenceReportFilters()
        
        {
            try
            {
                var sql = @"select ei.SystemId EmployeeId,DE.UserName Designation,ei.EmployeeName,S.UserName Section,SS.UserName SubSection,D.UserName Department
                            ,RG.UserName ResidenceGroup,RM.Id ResidenceId,RM.ResidenceNumber,RM.[Block],RM.ResidentType,RM.ResidenceSubCategory
							,E.UserName Entity

							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.DesignationSystemID
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetRSAFiltersViewData(Dictionary<string, string> parameters)
        {
            return Json(rsl.GetRSAFiltersViewData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ResidenceStatusAllocationReport(string employeeId, string SheetName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();

                string fileName = "";
                fileName = CreateResidenceStatusAllocationReportSheet(employeeId, "ResidenceStatusAllocation");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CreateResidenceStatusAllocationReportSheet(string employeeId, string SheetName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var data = GetResidenceStatusAllocationSql(employeeId);

            var sheet = workbook.Worksheets[0];

            #region sheet1
            sheet.Name = "Residence Status Allocation Report";

            int ROW = 7;
            int endCol = 1;
            int COL = 1;

            #region Grid Headers

            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Category Id", 10, ExcelHAlign.HAlignLeft);
            //int ColEmployeeCategoryId = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To", 10, ExcelHAlign.HAlignLeft);
            int ColTo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available", 10, ExcelHAlign.HAlignLeft);
            int ColAvailable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 20, ExcelHAlign.HAlignLeft);
            int ColEmployeeCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Given/Legal Designation", 25, ExcelHAlign.HAlignLeft);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmployeeId", 13, ExcelHAlign.HAlignLeft);
            int ColEmployeeId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee", 25, ExcelHAlign.HAlignLeft);
            int ColEmployee = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 13, ExcelHAlign.HAlignLeft);
            int ColSkill = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 18, ExcelHAlign.HAlignLeft);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 18, ExcelHAlign.HAlignLeft);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 25, ExcelHAlign.HAlignLeft);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 15, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Id", 25, ExcelHAlign.HAlignLeft);
            int ColEntityResidenceId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 25, ExcelHAlign.HAlignLeft);
            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 18, ExcelHAlign.HAlignLeft);
            int ColResidenceGroup = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Number", 18, ExcelHAlign.HAlignLeft);
            int ColResidenceNumber = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 10, ExcelHAlign.HAlignLeft);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 15, ExcelHAlign.HAlignLeft);
            int ColResidentType = COL;
            

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 18, ExcelHAlign.HAlignLeft);
            int ColEmployeeStatus = COL;

            endCol = COL;
            #endregion Headers


            sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            ROW++;
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                //sheet[ROW, ColEmployeeCategoryId].Text = data.Rows[i]["EmployeeCategoryId"].ToString();
                sheet[ROW, ColTo].Text = data.Rows[i]["To"].ToString();
                sheet[ROW, ColAvailable].Number = clsStaticInfo.dbl(data.Rows[i]["Available"].ToString());
                sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColEmployeeId].Text = data.Rows[i]["EmployeeId"].ToString();
                sheet[ROW, ColEmployee].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();

                sheet[ROW, ColEntityResidenceId].Text = data.Rows[i]["ResidenceId"].ToString();
                sheet[ROW, ColResidenceGroup].Text = data.Rows[i]["ResidenceGroup"].ToString();
                sheet[ROW, ColResidenceNumber].Text = data.Rows[i]["ResidenceNumber"].ToString();
                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();

                sheet.Range[ROW, ColTo, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColTo, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();

                sheet.Range[ROW, ColTo, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColTo, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            ROW++;

            //if (FromDate != "" && ToDate != "")
            //{


            //    report.SetText(ref sheet, ROW, Convert.ToInt32(ColBaseAmount) - 1, "Total");
            //    sheet.Range[ROW, Convert.ToInt32(ColBaseAmount) - 1].CellStyle.Font.Bold = true;
            //    //sheet.Range[1, ROW, Convert.ToInt32(ColTotalMaterialTranAmount) - 1, ROW].Merge();
            //    object sumObject;

            //    sumObject = data.Compute("Sum(BaseAmount)", "");
            //    sheet.Range[ROW, Convert.ToInt32(ColBaseAmount)].CellStyle.Font.Bold = true;
            //    report.SetText(ref sheet, ROW, Convert.ToInt32(ColBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
            //    sheet.Range[ROW, Convert.ToInt32(ColBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //    sheet.Range[ROW, Convert.ToInt32(ColBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //    sumObject = data.Compute("Sum(TaxAmount)", "");
            //    sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].CellStyle.Font.Bold = true;
            //    report.SetText(ref sheet, ROW, Convert.ToInt32(ColTaxAmount), Convert.ToDouble(sumObject).ToString("0.##"));
            //    sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //    sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

            //}

            endRow = ROW - 1;
            endRow = ROW - 1;

            #endregion sheet


            sheet.Name = SheetName;
            sheet.UsedRange.WrapText = true;
            sheet.IsGridLinesVisible = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            report.PlantHeader(ref sheet, ROW, SheetName, identity.PlantId);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
            workbook.Version = ExcelVersion.Excel2016;

            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        public DataTable GetResidenceStatusAllocationSql(string employeeId)
        {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var str = @"select DGM.EmployeeCategoryId,DGM.EmployeeCategory,'' [To],
							Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0),
							ei.SystemId EmployeeId,
							DE.UserName Designation,
							ei.EmployeeName,
							S.UserName Section,
							SS.UserName SubSection,
							D.UserName Department,
                            RG.UserName ResidenceGroup,
							RM.Id ResidenceId,RM.ResidenceNumber,RM.[Block],RM.ResidentType,RM.ResidenceSubCategory,
							E.UserName Entity
							,P.PaymentLink Skill,ei.EmployeeStatus
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = ei.SectionId
                            left join org.SubSection SS on SS.Id = ei.SubSectionId
                            left join org.Department D on D.Id = ei.DepartmentId
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=ei.PositionID

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id

                            where ei.SystemId in(" + employeeId + @") ";

                
                    return _sqlRepository.GetDataTable(str);
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region -- Residence Status Allocation



        [Authorize, HttpPost]
        public ActionResult XlsResidenceAllocationReport(Dictionary<string, string> parameters)
        {
            try
            {
                var workbook = ResidenceReport(parameters);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "ResidenceAllocationReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook ResidenceReport(Dictionary<string, string> parameters)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var data = rsl.residenceAllocationReport(parameters);

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Residence Status Allocation";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            //report.SetHeaderText(ref sheet, ROW, COL, "EmployeeCode", 20, ExcelHAlign.HAlignCenter);
            //int ColEmployeeCode = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "EmployeeName", 20, ExcelHAlign.HAlignCenter);
            //int ColEmployeeName = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Location", 12, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;

            

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Sub Category", 12, ExcelHAlign.HAlignCenter);
            int ColSubCategogry = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Type", 12, ExcelHAlign.HAlignCenter);
            int ColType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 20, ExcelHAlign.HAlignCenter);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Floor", 20, ExcelHAlign.HAlignCenter);
            int ColFloor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Number", 12, ExcelHAlign.HAlignCenter);
            int ColNumber = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rooms", 12, ExcelHAlign.HAlignCenter);
            int ColRooms = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vacancy", 20, ExcelHAlign.HAlignCenter);
            int ColVacancy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Occupied", 20, ExcelHAlign.HAlignCenter);
            int ColOccupied = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available", 20, ExcelHAlign.HAlignCenter);
            int ColAvailable = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[4];

            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();
                sheet[ROW, ColEmpCategory].Text = data.Rows[i]["EmployeeType"].ToString();
                sheet[ROW, ColSubCategogry].Text = data.Rows[i]["ResidenceSubCategory"].ToString();
                sheet[ROW, ColType].Text = data.Rows[i]["ResidentType"].ToString();
                //sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                //sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();


                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColFloor].Text = data.Rows[i]["Floor"].ToString();
                sheet[ROW, ColNumber].Text = data.Rows[i]["ResidenceNumber"].ToString();
                sheet[ROW, ColRooms].Number = clsStaticInfo.dbl(data.Rows[i]["Rooms"].ToString());
                sheet[ROW, ColVacancy].Number = clsStaticInfo.dbl(data.Rows[i]["Vacancy"].ToString());
                sheet[ROW, ColOccupied].Number = clsStaticInfo.dbl(data.Rows[i]["Occupied"].ToString());
                sheet[ROW, ColAvailable].Number = clsStaticInfo.dbl(data.Rows[i]["Available"].ToString());
                
                arr[0] += clsStaticInfo.dbl(data.Rows[i]["Rooms"].ToString());
                arr[1] += clsStaticInfo.dbl(data.Rows[i]["Vacancy"].ToString());
                arr[2] += clsStaticInfo.dbl(data.Rows[i]["Occupied"].ToString());
                arr[3] += clsStaticInfo.dbl(data.Rows[i]["Available"].ToString());


                ROW++;


            }

            //ROW++;

            sheet[ROW, ColLocation].Text = "Grand Total";
            sheet[ROW, ColRooms].Number = arr[0];
            sheet[ROW, ColVacancy].Number = arr[1];
            sheet[ROW, ColOccupied].Number = arr[2];
            sheet[ROW, ColAvailable].Number = arr[3];

            sheet.Range[ROW, ColLocation, ROW, endCol].CellStyle.Font.Bold = true;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "Residence Status Allocation", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }


        #endregion -- Residence Status Allocation
        [Authorize, HttpPost]
        public ActionResult employeeCurrrentStatus()
        {
            try
            {
                return Json(rsl.employeeCurrrentStatus(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        #region RESIDENCE MASTER REPORT
        [Authorize, HttpPost]
        public ActionResult XlsResidenceMaterReport(string empCurrentStatus)
        {
            try
            {
                var workbook = ResidenceMasterReport(empCurrentStatus);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "ResidenceMasterReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        private IWorkbook ResidenceMasterReport(string empCurrentStatus)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

          
                var data = rsl.residencemasterReport(empCurrentStatus);
            

            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Residence Master";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Residence Id", 12, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 26, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 12, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 12, ExcelHAlign.HAlignCenter);
            int ColLegalDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceGroup = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Category", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Sub Category", 12, ExcelHAlign.HAlignCenter);
            int ColResSubCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Location", 12, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 12, ExcelHAlign.HAlignCenter);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Floor", 12, ExcelHAlign.HAlignCenter);
            int ColFloor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Number", 12, ExcelHAlign.HAlignCenter);
            int ColResNmbr = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vacancy", 12, ExcelHAlign.HAlignCenter);
            int ColVacancy = COL;
            COL++;

           

            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {

                //sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColEmpCategory].Text = data.Rows[i]["Employee Category"].ToString();
                sheet[ROW, ColResidenceGroup].Text = data.Rows[i]["Residence Group"].ToString();
                sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();
                sheet[ROW, ColResidenceCategory].Text = data.Rows[i]["ResidenceCategory"].ToString();
                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColFloor].Text = data.Rows[i]["Floor"].ToString();
                sheet[ROW, ColResNmbr].Number = clsStaticInfo.dbl(data.Rows[i]["ResidenceNumber"].ToString());
                sheet[ROW, ColResSubCat].Text = data.Rows[i]["ResidenceSubCategory"].ToString();
                sheet[ROW, ColResidenceType].Text = data.Rows[i]["ResidentType"].ToString();
                sheet[ROW, ColVacancy].Number = clsStaticInfo.dbl(data.Rows[i]["Vacancy"].ToString());
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColDOJ].Text =data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["Sub Section"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["Legal Designation"].ToString();
                
                ROW++;


            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "ResidenceMaster", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion RESIDENCE MASTER REPORT

        #region RESIDENCE MASTER REPORT ALL
        [Authorize, HttpPost]
        public ActionResult XlsAllResidenceMaterReport()
        {
            try
            {
                var workbook = allResidenceMasterReport();

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "ResidenceMasterReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        private IWorkbook allResidenceMasterReport()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsl.allresidencemasterReport();


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Residence Master";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Residence Id", 12, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 26, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 12, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 12, ExcelHAlign.HAlignCenter);
            int ColLegalDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceGroup = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Category", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Sub Category", 12, ExcelHAlign.HAlignCenter);
            int ColResSubCat = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Location", 12, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 12, ExcelHAlign.HAlignCenter);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Floor", 12, ExcelHAlign.HAlignCenter);
            int ColFloor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Number", 12, ExcelHAlign.HAlignCenter);
            int ColResNmbr = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vacancy", 12, ExcelHAlign.HAlignCenter);
            int ColVacancy = COL;
            COL++;



            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColId].Text = data.Rows[i]["Id"].ToString();
                sheet[ROW, ColEmpCategory].Text = data.Rows[i]["Employee Category"].ToString();
                sheet[ROW, ColResidenceGroup].Text = data.Rows[i]["Residence Group"].ToString();
                sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();
                sheet[ROW, ColResidenceCategory].Text = data.Rows[i]["ResidenceCategory"].ToString();
                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColFloor].Text = data.Rows[i]["Floor"].ToString();
                sheet[ROW, ColResNmbr].Text = data.Rows[i]["ResidenceNumber"].ToString();
                sheet[ROW, ColResSubCat].Text = data.Rows[i]["ResidenceSubCategory"].ToString();
                sheet[ROW, ColResidenceType].Text = data.Rows[i]["ResidentType"].ToString();
                sheet[ROW, ColVacancy].Text = data.Rows[i]["Vacancy"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["Sub Section"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["Legal Designation"].ToString();

                ROW++;


            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "ResidenceMaster", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion RESIDENCE MASTER REPORT ALL

        [Authorize, HttpPost]
        public ActionResult gridViewResidenceMAster()
        {
            try
            {
                return Json(rsl.gridViewResidenceMAster(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Detail Residence Status
        [Authorize, HttpPost]
        public ActionResult XlsDetailResidenceStatus(string PartialVacantFullyOccupied)
        {
            try
            {
                ReportUtility oRU = null;
                oRU = new ReportUtility();
                var workbook = DetailResidenceStatus(PartialVacantFullyOccupied, oRU);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "DetailResidenceStatus.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        [HttpPost]
        private IWorkbook DetailResidenceStatus(string PartialVacantFullyOccupied, ReportUtility oRU)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.detailResidenceStatusReport(PartialVacantFullyOccupied);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Detail Residence Status";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Residence Id", 12, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Location", 12, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 12, ExcelHAlign.HAlignCenter);
            int ColResidentType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Category", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 12, ExcelHAlign.HAlignCenter);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Floor", 12, ExcelHAlign.HAlignCenter);
            int ColFloor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Number", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceNumber = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vacancy", 12, ExcelHAlign.HAlignCenter);
            int ColVacancy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Occupied", 12, ExcelHAlign.HAlignCenter);
            int ColOccupied = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available", 12, ExcelHAlign.HAlignCenter);
            int ColAvailable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 26, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;

            COL++;
            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Activity", 12, ExcelHAlign.HAlignCenter);
            int ColActivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 12, ExcelHAlign.HAlignCenter);
            int ColSkill = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
            int ColProcess = COL;
            COL++;
            
            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 12, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOS", 12, ExcelHAlign.HAlignCenter);
            int ColDOS = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;
            COL++;

            

            //report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 12, ExcelHAlign.HAlignCenter);
            //int ColLegalDesignation = COL;
            //COL++;


            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceGroup = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Residence Category", 12, ExcelHAlign.HAlignCenter);
            //int ColResidenceCategory = COL;
            //COL++;

           

            


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColId].Text = data.Rows[i]["ResidenceId"].ToString();
                sheet[ROW, ColEmpCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                sheet[ROW, ColResidenceGroup].Text = data.Rows[i]["ResidenceGroup"].ToString();
                sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();
                sheet[ROW, ColResidenceCategory].Text = data.Rows[i]["ResidenceCategory"].ToString();
                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColFloor].Text = data.Rows[i]["Floor"].ToString();
                sheet[ROW, ColResidenceNumber].Text = data.Rows[i]["ResidenceNumber"].ToString();
               
                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();
                sheet[ROW, ColVacancy].Number = clsStaticInfo.dbl(data.Rows[i]["Vacancy"].ToString());
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColEmployeeCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                //sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["LegalDesignation"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();
                
                sheet[ROW, ColOccupied].Number = clsStaticInfo.dbl(data.Rows[i]["Occupied"].ToString());
                sheet[ROW, ColAvailable].Number = clsStaticInfo.dbl(data.Rows[i]["Available"].ToString());
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColActivity].Text = data.Rows[i]["Activity"].ToString();
                sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                sheet[ROW, ColResidenceCategory].Text = data.Rows[i]["ResidenceCategory"].ToString();

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;

            sheet.Range[startRow, COL].NumberFormat = oRU.NumberFormatDecimalTwo();
            sheet.Range["A4" + ":" + oRU.GetColumnNameForXls(COL) + "4"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range["A4" + ":" + oRU.GetColumnNameForXls(COL) + "4"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;

            oRU.PageSetup(ref sheet, 4, ExcelPageOrientation.Portrait);
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];
            //for(int i = 0; i <= sheet.Range.Row; i++)
            // {
            //     for (int j = 0; i <= sheet.Range.Column; j++)
            //     {
            //         sheet.Range[startRow, i, startRow, endCol][startRow, j, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //     }
            // }

            // sheet.Range[startRow, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 2, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 3, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 4, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 5, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 6, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 7, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 8, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 9, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            // sheet.Range[startRow, 10, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;


            // sheet.Range[startRow, 1, startRow, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;


            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "DetailResidenceStatus", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }


        #endregion Detail Residence Status

        #region Pending for unallcation
        [Authorize, HttpPost]
        public ActionResult XlsPendingForUnallocation()
        {
            try
            {
                var workbook = pendingForUnAllocationReport();

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "PendingForUnAllocation.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook pendingForUnAllocationReport()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.pendingForUnAllocationReport();


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "PendingForUnallocation";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Residence Id", 12, ExcelHAlign.HAlignCenter);
            int ColId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Location", 12, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 12, ExcelHAlign.HAlignCenter);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Floor", 12, ExcelHAlign.HAlignCenter);
            int ColFloor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Number", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceNumber = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vacancy", 12, ExcelHAlign.HAlignCenter);
            int ColVacancy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Occupied", 12, ExcelHAlign.HAlignCenter);
            int ColOccupied = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available", 12, ExcelHAlign.HAlignCenter);
            int ColAvailable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 26, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

          
            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Activity", 12, ExcelHAlign.HAlignCenter);
            int ColActivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 12, ExcelHAlign.HAlignCenter);
            int ColSkill = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
            int ColProcess = COL;
            COL++;
          
            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 12, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOS", 12, ExcelHAlign.HAlignCenter);
            int ColDOS = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 12, ExcelHAlign.HAlignCenter);
            int ColResidentType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Category", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceGroup = COL;
            COL++;

          
            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {

                sheet[ROW, ColId].Text = data.Rows[i]["ResidenceId"].ToString();
                sheet[ROW, ColEmpCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                sheet[ROW, ColResidenceGroup].Text = data.Rows[i]["ResidenceGroup"].ToString();
                sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();
                sheet[ROW, ColResidenceCategory].Text = data.Rows[i]["ResidenceCategory"].ToString();
                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColFloor].Text = data.Rows[i]["Floor"].ToString();
                sheet[ROW, ColResidenceNumber].Text = data.Rows[i]["ResidenceNumber"].ToString();

                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();
                sheet[ROW, ColVacancy].Number = clsStaticInfo.dbl(data.Rows[i]["Vacancy"].ToString());
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColEmployeeCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                //sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["LegalDesignation"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();

                sheet[ROW, ColOccupied].Number = clsStaticInfo.dbl(data.Rows[i]["Occupied"].ToString());
                sheet[ROW, ColAvailable].Number = clsStaticInfo.dbl(data.Rows[i]["Available"].ToString());
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColActivity].Text = data.Rows[i]["Activity"].ToString();
                sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                sheet[ROW, ColResidenceCategory].Text = data.Rows[i]["ResidenceCategory"].ToString();

                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];
            //for (int i = 0; i <= sheet.Range.Row; i++)
            //{
            //    for (int j = 0; i <= sheet.Range.Column; j++)
            //    {
            //        sheet.Range[startRow, i, startRow, endCol][startRow, j, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //    }
            //}

            sheet.Range[startRow, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 2, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 3, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 4, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 5, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 6, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 7, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 8, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 9, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 10, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;


            // sheet.Range[startRow, 1, startRow, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;


            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "DetailResidenceStatus", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        #endregion Pending for unallcation

        #region Residence Summary Report
        [Authorize, HttpPost]
        public ActionResult XlsResidenceSummary()
        {
            try
            {
                var workbook = ResidenceSummaryReport();

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "ResidenceSummary.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook ResidenceSummaryReport()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.ResidenceSummaryReport();


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Residence Summary Rport";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCtegory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Location", 12, ExcelHAlign.HAlignCenter);
            int ColLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Block", 12, ExcelHAlign.HAlignCenter);
            int ColBlock = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 12, ExcelHAlign.HAlignCenter);
            int ColResidentType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rooms", 12, ExcelHAlign.HAlignCenter);
            int ColRooms = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Vacancy", 12, ExcelHAlign.HAlignCenter);
            int ColVacancy = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Occupied", 12, ExcelHAlign.HAlignCenter);
            int ColOccupied = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available", 12, ExcelHAlign.HAlignCenter);
            int ColAvailable = COL;
            //COL++;


            ROW++;
            endCol = COL;
            #endregion Headers

            string EmpCategory = "";
            string Location = "";

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            int EmpCateRow = 0;
            int LocationRow = 0;
            

            double[] arr = new double[4];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                if(EmpCategory != data.Rows[i]["EmpCategory"].ToString())
                {
                    EmpCategory = data.Rows[i]["EmpCategory"].ToString();

                    sheet[ROW, ColEmployeeCtegory].Text = data.Rows[i]["EmpCategory"].ToString();

                    if (i != 0 && EmpCateRow != (ROW - 1))
                    {
                        sheet.Range[EmpCateRow, ColEmployeeCtegory, ROW - 1, ColEmployeeCtegory].Merge();
                        sheet.Range[EmpCateRow, ColEmployeeCtegory, ROW - 1, ColEmployeeCtegory].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    EmpCateRow = ROW;
                }

                if(Location != data.Rows[i]["Location"].ToString())
                {
                    Location = data.Rows[i]["Location"].ToString();
                    sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();

                    if(i != 0 && LocationRow != (ROW - 1))
                    {
                        sheet.Range[LocationRow, ColLocation, ROW - 1, ColLocation].Merge();
                        sheet.Range[LocationRow, ColLocation, ROW - 1, ColLocation].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    LocationRow = ROW;
                }
               
                sheet[ROW, ColLocation].Text = data.Rows[i]["Location"].ToString();
                sheet[ROW, ColEmployeeCtegory].Text = data.Rows[i]["EmpCategory"].ToString();
               
                sheet[ROW, ColBlock].Text = data.Rows[i]["Block"].ToString();
                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();
               
               
                sheet[ROW, ColRooms].Number = clsStaticInfo.dbl(data.Rows[i]["Rooms"].ToString());
                sheet[ROW, ColVacancy].Number = clsStaticInfo.dbl(data.Rows[i]["Capacity"].ToString());
               

                sheet[ROW, ColOccupied].Number = clsStaticInfo.dbl(data.Rows[i]["Allotted"].ToString());
                sheet[ROW, ColAvailable].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());


                arr[0] += clsStaticInfo.dbl(data.Rows[i]["Capacity"].ToString());
                arr[1] += clsStaticInfo.dbl(data.Rows[i]["Allotted"].ToString());
                arr[2] += clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                arr[3] += clsStaticInfo.dbl(data.Rows[i]["Rooms"].ToString());

               
                ROW++;

            }

            ROW++;
            
            sheet[ROW, ColEmployeeCtegory].Text = "Grand Total";
            sheet[ROW, ColVacancy].Number = arr[0];
            sheet[ROW, ColOccupied].Number = arr[1];
            sheet[ROW, ColAvailable].Number = arr[2];
            sheet[ROW, ColRooms].Number = arr[3];

            sheet.Range[ROW, ColEmployeeCtegory, ROW, endCol].CellStyle.Font.Bold = true;
            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];
            //for (int i = 0; i <= sheet.Range.Row; i++)
            //{
            //    for (int j = 0; i <= sheet.Range.Column; j++)
            //    {
            //        sheet.Range[startRow, i, startRow, endCol][startRow, j, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //    }
            //}

            //sheet.Range[startRow, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 2, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 3, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 4, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 5, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 6, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 7, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 8, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 9, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 10, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;


            // sheet.Range[startRow, 1, startRow, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;


            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "ResidenceSmmaryReport", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion Residence Summary Report

        #region Pending for allocation
        [Authorize, HttpPost]
        public ActionResult XlsPendingForAllocation()
        {
            try
            {
                var workbook = pendingForAllocationReport();

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "PendingForAllocation.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        private IWorkbook pendingForAllocationReport()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.pendingForAllocationReport();


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Pending For Allocation";

            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers
            

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 26, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 12, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 12, ExcelHAlign.HAlignCenter);
            int ColEmpCategory = COL;
            COL++;

            

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Activity", 12, ExcelHAlign.HAlignCenter);
            int ColActivity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Skill", 12, ExcelHAlign.HAlignCenter);
            int ColSkill = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Process", 12, ExcelHAlign.HAlignCenter);
            int ColProcess = COL;
            COL++;

           

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 12, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOS", 12, ExcelHAlign.HAlignCenter);
            int ColDOS = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Resident Type", 12, ExcelHAlign.HAlignCenter);
            int ColResidentType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Residence Group", 12, ExcelHAlign.HAlignCenter);
            int ColResidenceGroup = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            string Article = "";
            string LotNum = "";
            int ArtRow = 0;
            int LotRow = 0;

            double[] arr = new double[3];

            for (int i = 0; i < data.Rows.Count; i++)
            {

               
                sheet[ROW, ColEmpCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                sheet[ROW, ColResidenceGroup].Text = data.Rows[i]["ResidenceGroup"].ToString();
               
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColEmployeeCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                //sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["LegalDesignation"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
               
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColActivity].Text = data.Rows[i]["Activity"].ToString();
                sheet[ROW, ColSkill].Text = data.Rows[i]["Skill"].ToString();
                sheet[ROW, ColProcess].Text = data.Rows[i]["Process"].ToString();
                sheet[ROW, ColResidentType].Text = data.Rows[i]["ResidentType"].ToString();
                
                
                ROW++;

            }

            ROW++;

            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];
            //for (int i = 0; i <= sheet.Range.Row; i++)
            //{
            //    for (int j = 0; i <= sheet.Range.Column; j++)
            //    {
            //        sheet.Range[startRow, i, startRow, endCol][startRow, j, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //    }
            //}

            //sheet.Range[startRow, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 2, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 3, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 4, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 5, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 6, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 7, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 8, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 9, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[startRow, 10, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;


            // sheet.Range[startRow, 1, startRow, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

            sheet.Range[startRow - 1, 1, startRow, endCol].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            ReportUtility reportUtility = new ReportUtility();
            //reportUtility.CompanyHeader(ref sheet, endCol, "PendingForAllocation", identity.CompanyId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }
        #endregion Pending for allocation

        #region grid view 
        [HttpPost, Authorize]
        public JsonResult detailResidenceStatusGrid(string PartialVacantFullyOccupied)
        {
            try
            {
                return Json(rsr.detailResidenceStatusGrid(PartialVacantFullyOccupied), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult pendingForAllocationGrid()
        {
            try
            {
                var jsondata = Json(rsr.pendingForAllocationGrid(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult pendingForUnAllocationGrid()
        {
            try
            {
                return Json(rsr.pendingForUnAllocationGrid(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult residenceSummarGrid()
        {
            try
            {
                return Json(rsr.residenceSummarGrid(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion grid view 
    }
}