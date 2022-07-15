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
        ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();
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
        public JsonResult getemployeeDataList(string plantId)
        {
            return Json(rsl.getemployeeDataList(plantId), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetViewData(Dictionary<string,string> parameters)
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

        //[HttpPost]
        //public ActionResult getData()
        //{
        //    try
        //    {
        //        return Json(rsl.getData(), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

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

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category Id", 10, ExcelHAlign.HAlignLeft);
            int ColEmployeeCategoryId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "To", 10, ExcelHAlign.HAlignLeft);
            int ColTo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available", 12, ExcelHAlign.HAlignLeft);
            int ColAvailable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 13, ExcelHAlign.HAlignLeft);
            int ColEmployeeCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Given/Legal Designation", 13, ExcelHAlign.HAlignLeft);
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

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 13, ExcelHAlign.HAlignLeft);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 13, ExcelHAlign.HAlignLeft);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 13, ExcelHAlign.HAlignLeft);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 25, ExcelHAlign.HAlignLeft);
            int ColEntity = COL;
        
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
                sheet[ROW, ColEmployeeCategoryId].Text = data.Rows[i]["EmployeeCategoryId"].ToString();
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
               
                sheet.Range[ROW, ColEmployeeCategoryId, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, ColEmployeeCategoryId, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
            }

            //ROW++;

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
							,P.PaymentLink Skill
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

    }
}