#region Using
using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class RouteEmployeeController : BaseController
    {
        #region Constructor
        private readonly IRouteEmployeeService _routeEmployeeService;
        private readonly ISqlRepository _sqlRepository;
        EmployeeTransport ET = new EmployeeTransport();
        public RouteEmployeeController(
              IRouteEmployeeService routeEmployeeService,
              ISqlRepository sqlRepository
            )
        {
            _routeEmployeeService = routeEmployeeService;
            _sqlRepository = sqlRepository;

        }
        #endregion
        #region -- Pages
        public ActionResult Report()
        {
            return View();
        }
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

       #region -- Operations 

        [HttpPost,Authorize]
        public ActionResult SaveUnAssign(List<UARouteEmployeeList> UArouteEmployeeList)
        {
            try
            {
                foreach (var item in UArouteEmployeeList)
                {
                    if (item.RouteId != null && item.StoppageId == null)
                    {
                        Exception ex = new Exception("Please Select Stopage");
                        throw (ex);
                    }
                    

                    if (item.RouteId == null /*&& item.UARouteDownGridId == null*/)
                    {
                        Exception ex = new Exception("Please Select Route");
                        throw (ex);
                    }
                }
                SaveUARouteEmployeeSepLis(UArouteEmployeeList);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void SaveUARouteEmployeeSepLis(List<UARouteEmployeeList> UArouteEmployeeList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {

                foreach (var item in UArouteEmployeeList)
                {

                    string sql = "SELECT * FROM [TRN].[RouteEmployee] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    DataView DvMaster = new DataView(dsMaster.Tables[0]);

                    if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[TRN].[RouteEmployee]", out sID);

                        dr["Id"] = "RE" + sID;
                        dr["EmployeeId"] = item.EmployeeId;

                        dr["RouteId"] = item.RouteId;
                        dr["StoppageId"] = item.StoppageId;
                        dr["ShiftId"] = item.ShiftId;

                        //dr["UpRouteId"] = item.UARouteUpGridId;
                        //dr["UpStoppageId"] = item.UAStopageUpGridId;

                        //dr["DownRouteId"] = item.UARouteDownGridId;
                        //dr["DownStoppageId"] = item.UAStopageDownGridId;

                        dr["PlantId"] = identity.PlantId;
                        dr["Active"] = true;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["EmployeeId"] = item.EmployeeId;

                        dr["RouteId"] = item.RouteId;
                        dr["StoppageId"] = item.StoppageId;
                        dr["ShiftId"] = item.ShiftId;

                        //dr["UpRouteId"] = item.UARouteUpGridId;
                        //dr["UpStoppageId"] = item.UAStopageUpGridId;

                        //dr["DownRouteId"] = item.UARouteDownGridId;
                        //dr["DownStoppageId"] = item.UAStopageDownGridId;

                        dr["PlantId"] = identity.PlantId;
                        dr["Active"] = true;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region -- New
       
        //Route Emp start
        [HttpGet, Authorize]
        public ActionResult GetRouteEmployeesData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select RS.Id TripId,RS.TripNo,R.Id RouteId,R.StandardName Route,TD.Id TransportId,TD.TransportUserName+'-'+TD.TransportNo Transport 
					                        --,RSD.UpDown
											,REPLACE(REPLACE(
                                                    STUFF((select distinct ','+A.UpDown +':'+ISNULL(format(A.StartTime,'hh:mm tt'),'')StartTime from
                                                        RouteScheduleChild A
                                                            
                                                            where A.RouteScheduleId=RS.Id and A.UpDown='Up'  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''
															)
                                                                    ,'&amp;','&'), 'amp;', '') UpStart
											,REPLACE(REPLACE(
                                                    STUFF((select distinct ','+A.UpDown +':'+ISNULL(format(A.StartTime,'hh:mm tt'),'')StartTime from
                                                        RouteScheduleChild A
                                                            
                                                            where A.RouteScheduleId=RS.Id and A.UpDown='Down'  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''
															)
                                                                    ,'&amp;','&'), 'amp;', '') DownStart
											,R.[From],R.[To],SD.UserName [Shift],TD.Capacity Vacancy,TD.PlanCapacity
					                        ,isnull(O.Alloted,0)Alloted,R.PlantId
					                        ,Balance=TD.PlanCapacity-isnull(O.Alloted,0)
										
					                        ,R.Remarks

					                        from RouteSchedule RS
					                        left join [MST].[Route] R on R.Id=RS.RouteId 
					                        left join TransportDetail TD on TD.Id=RS.TransportId
					                        left join ShiftDefination SD on SD.SystemID=RS.ShiftId
					                        LEFT JOIN(select COUNT(A.EmployeeSystemId) Alloted,A.TripId
															
															from dbo.EmployeeTransportAllocation A
															where A.AssignStatus=1
									                        Group BY TripId) O ON O.TripId=RS.Id

											Where R.PlantId ='" + identity.PlantId+"'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet, Authorize]
        public JsonResult getemployeeDataListRouteEmp(string plantId)
        {
            return Json(ET.GetemployeeDataListRouteEmp(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getemployeeListRoute(string plantId)
        {
            return Json(ET.GetemployeeListRoute(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTransportSummaryData()
        {
            try
            {
                var sql = @"select O.TransportGroup,O.Stoppage,R.StandardName Route,TD.Id TransportId,TD.TransportNo,TD.TransportUserName Transport,RS.Id TripId,RS.TripNo
											,TD.Capacity Vacancy,TD.PlanCapacity,isnull(O.Alloted,0)Alloted,Balance=TD.PlanCapacity-isnull(O.Alloted,0)
											,O.EmployeeCode,O.EmployeeName,O.EmployeeStatus,O.EmployeeCurrentStatus,o.BudgatedShift,O.AssignedShift
											,O.InTime,O.TBS,O.LAbs
											,O.DOJ
                                            ,Skill =isnull(O.OperationMaster,O.OperationVariation),O.GivenDesignation
											,O.Section,O.SubSection,O.Department,O.EntityName,O.Plant,O.EID

					                        from RouteSchedule RS
					                        left join [MST].[Route] R on R.Id=RS.RouteId 
					                        left join TransportDetail TD on TD.Id=RS.TransportId
					                        LEFT JOIN(select COUNT(A.EmployeeSystemId) Alloted,A.TripId,EMP.EmployeeName,EMP.EmployeeCode,EMP.SystemId EID
															,Emp.EmployeeStatus, Emp.EmployeeCurrentStatus
															,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,PR.PaymentLink Skill,DEG.UserName GivenDesignation
															,S.UserName Section,SS.UserName SubSection,DEPT.UserName Department,E.UserName EntityName
															,PL.UserName Plant,TG.UserName TransportGroup,A.AssignStatus,ST.UserName Stoppage
															,FORMAT(apd.InTime,'hh:mm:tt') InTime
															,MSD.UserName BudgatedShift,ESD.UserName AssignedShift
															,OV.UserName OperationVariation,OM.UserName OperationMaster
															,ISNULL(TE.TBSEmp,0) TBS,ISNULL(LA.LONGEmp,0) LAbs

															from dbo.EmployeeTransportAllocation A
															left join EmployeeInformation EMP on EMP.SystemId=A.EmployeeSystemId
															LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
															LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
															LEFT JOIN ShiftDefination MSD on MSD.SystemID=PMB.ShiftDefinationId 
															LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
															LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
															--LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
															LEFT JOIN HKP.LegalDesignation DEG ON EMP.LegalDesignationId=DEG.Id
															LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
															LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
															LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
															left join [dbo].[TransportGroup] TG on TG.Id=EMP.TransportGroupId
                                                            left join HKP.Stoppage ST on ST.Id=A.StoppageId
															LEFT JOIN dbo.AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId AND apd.WorkDate=FORMAT(GetDate(),'dd-MMM-yyyy')
															LEFT JOIN ShiftDefination ESD on ESD.SystemID=apd.ShiftSystemID 
															LEFT JOIN MST.OperationVariation OV on OV.Id=EMP.OperationVariationId
															LEFT JOIN MST.OperationMaster OM on OM.Id=EMP.OperationMasterID 
															LEFT JOIN (SELECT COUNT(SystemId) TBSEmp,SystemId From EmployeeInformation Where EmployeeStatus='Active' AND EmployeeCurrentStatus='TBS' AND BudgetCode IS NOT NULL GROUP BY SystemId) TE ON TE.SystemId=A.EmployeeSystemId
															LEFT JOIN (SELECT COUNT(SystemId) LONGEmp,SystemId From EmployeeInformation Where EmployeeStatus='Active' AND EmployeeCurrentStatus='LONG ABSENTEEISM' AND BudgetCode IS NOT NULL GROUP BY SystemId) LA ON LA.SystemId=A.EmployeeSystemId

									                        Group BY TripId,Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode
															,Emp.EmployeeStatus, Emp.EmployeeCurrentStatus
															,emp.DOJ,PR.PaymentLink,DEG.UserName
															,S.UserName,SS.UserName,DEPT.UserName,E.UserName
															,PL.UserName,TG.UserName,A.AssignStatus,ST.UserName,apd.InTime
															,MSD.UserName,ESD.UserName
															,OV.UserName,OM.UserName,TE.TBSEmp,LA.LONGEmp) O ON O.TripId=RS.Id
															where O.AssignStatus=1";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //Route Emp end

        [HttpGet, Authorize]
        public JsonResult viewUnassign(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(ET.getviewUnassign(PlantId), JsonRequestBehavior.AllowGet);
        }


        #region Save Operations
        [HttpPost]
        public JsonResult employeeTransportAllocationSave(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                ET.SaveEmployeeTransportAllocation(EmployeeList);
                return Json(new { Data = EmployeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetStopageInformation(string routeId)
        {
            string sql = @"select S.Id,S.UserName
                                        from [MST].[RouteStoppage] RS
                                        left join [HKP].[Stoppage] S on S.Id=RS.StoppageId
                                        where RS.RouteId ='"+routeId+"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public JsonResult SaveUnassignData(List<Dictionary<string, object>> employeeList)
        {

            try
            {

                ET.SaveUnassignData(employeeList);
                return Json(new { Data = employeeList, Message = AplosMessage.Insert });
                //return Json(new { Error = "No", Data = rsl.Save( EmployeeList, ResidenceMasterId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetBusVerificationReport(ReportFormat reportFormat)
        {
            try
            {
                string fileName = "";

                IWorkbook workbook = GetBusVerificationReportWorkbook("Data");
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "BusVerificationReport";
                // return RenderReportAsPdf(workbook, reportFileName);
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkbook GetBusVerificationReportWorkbook(string SheetName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];
                DataTable dtOrder = null;
                string sql = "";
                sql = @"SELECT BV.EmpSystemID,EI.EmployeeCode,FORMAT(BV.WorkDate,'dd-MMM-yyyy')WorkDate,ISNULL(format(BV.InTime,'dd-MMM-yyyy hh:mm tt'),'') as InTime
,ISNULL(format(BV.OutTime,'dd-MMM-yyyy hh:mm tt'),'') OutTime,BV.AddedBy,FORMAT(BV.AddedDate,'dd-MMM-yyyy')AddedDate,BV.UpdatedBy,FORMAT(BV.UpdatedDate,'dd-MMM-yyyy')UpdatedDate
,ST.UserName Stoppage,TD.TransportUserName Transport,R.StandardName [Route],S.UserName Section,SS.UserName SubSection,DEPT.UserName Department
FROM dbo.BusVerification BV
LEFT JOIN dbo.EmployeeTransportAllocation ETA ON ETA.EmployeeSystemId=EmpSystemId
LEFT JOIN HKP.Stoppage ST on ST.Id=ETA.StoppageId
LEFT JOIN RouteSchedule RS on RS.Id = ETA.TripId
LEFT JOIN MST.Route R on R.Id = RS.RouteId
LEFT JOIN TransportDetail TD on TD.Id = RS.TransportId
LEFT JOIN EmployeeInformation EI on EI.SystemId = ETA.EmployeeSystemId
LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
LEFT JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
LEFT JOIN ORG.Department DEPT ON EI.DepartmentId=DEPT.Id
Where ETA.AssignStatus=1";


                dtOrder = _sqlRepository.GetDataTable(sql);


                if (dtOrder.Rows.Count == 0)
                {
                    throw new Exception("No Data Found.");
                }
                ReportUtility reportUtility = new ReportUtility();

                int ROW = 4; int COL = 1;
                
                ROW++;
                #region ColumnsHeader

                sheet[ROW, COL].Text = "SNo"; sheet[ROW, COL].ColumnWidth = 5; int colSL = COL; COL++;
                sheet[ROW, COL].Text = "Emp SystemID"; sheet[ROW, COL].ColumnWidth = 10; int colEID = COL; COL++;
                sheet[ROW, COL].Text = "Employee Code"; sheet[ROW, COL].ColumnWidth = 11; int colEC = COL; COL++;
                sheet[ROW, COL].Text = "Work Date"; sheet[ROW, COL].ColumnWidth = 8; int colWD = COL; COL++;
                sheet[ROW, COL].Text = "InTime"; sheet[ROW, COL].ColumnWidth = 15; int colIT = COL; COL++;
                sheet[ROW, COL].Text = "OutTime"; sheet[ROW, COL].ColumnWidth = 15; int colOT = COL; COL++;
                sheet[ROW, COL].Text = "AddedBy"; sheet[ROW, COL].ColumnWidth = 13.50; int colAB = COL; COL++;
                sheet[ROW, COL].Text = "AddedDate"; sheet[ROW, COL].ColumnWidth = 8; int colAD = COL; COL++;                
                sheet[ROW, COL].Text = "UpdatedBy"; sheet[ROW, COL].ColumnWidth = 13.50; int colUB = COL; COL++;
                sheet[ROW, COL].Text = "UpdatedDate"; sheet[ROW, COL].ColumnWidth = 8; int colUD = COL; COL++;                
                sheet[ROW, COL].Text = "Route"; sheet[ROW, COL].ColumnWidth = 13.50; int colRoute = COL; COL++;
                sheet[ROW, COL].Text = "Stoppage"; sheet[ROW, COL].ColumnWidth = 15; int colStoppage = COL; COL++;
                sheet[ROW, COL].Text = "TransportNo"; sheet[ROW, COL].ColumnWidth = 13.50; int colTN = COL; COL++;
                sheet[ROW, COL].Text = "Department"; sheet[ROW, COL].ColumnWidth = 12; int colDP = COL; COL++;                
                sheet[ROW, COL].Text = "Section"; sheet[ROW, COL].ColumnWidth = 13.50; int colSection = COL; COL++;
                sheet[ROW, COL].Text = "SubSection"; sheet[ROW, COL].ColumnWidth = 12; int colSubSection = COL; 


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                #endregion columns

                ROW++;
                int startRow = ROW;
                int cnt = 0;
                #region DataPlot
                for (int i = 0; i < dtOrder.Rows.Count; i++)
                {
                    cnt++;
                    sheet[ROW, colSL].Number = Library.Service.Extension.clsStaticInfo.dbl(cnt.ToString());
                    sheet[ROW, colEID].Text = dtOrder.Rows[i]["EmpSystemID"].ToString();
                    sheet[ROW, colEC].Text = dtOrder.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, colWD].Text = dtOrder.Rows[i]["WorkDate"].ToString();
                    sheet[ROW, colIT].Text = dtOrder.Rows[i]["InTime"].ToString();
                    sheet[ROW, colOT].Text = dtOrder.Rows[i]["OutTime"].ToString();
                    sheet[ROW, colAB].Text = dtOrder.Rows[i]["AddedBy"].ToString();
                    sheet[ROW, colAD].Text = dtOrder.Rows[i]["AddedDate"].ToString(); 
                    sheet[ROW, colUB].Text = dtOrder.Rows[i]["UpdatedBy"].ToString();
                    sheet[ROW, colUD].Text = dtOrder.Rows[i]["UpdatedDate"].ToString();
                    sheet[ROW, colStoppage].Text = dtOrder.Rows[i]["Stoppage"].ToString();
                    sheet[ROW, colTN].Text = dtOrder.Rows[i]["Transport"].ToString();
                    sheet[ROW, colRoute].Text = dtOrder.Rows[i]["Route"].ToString();
                    sheet[ROW, colSection].Text = dtOrder.Rows[i]["Section"].ToString();
                    sheet[ROW, colSubSection].Text = dtOrder.Rows[i]["SubSection"].ToString();
                    sheet[ROW, colDP].Text = dtOrder.Rows[i]["Department"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                #endregion
                int edCRow = ROW;
                              
                edCRow++;

                #region ReportHeader
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();


                reportUtility.CompanyHeader(ref sheet, 3, "Bus Verification Report", identity.CompanyId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                sheet.PageSetup.CenterHorizontally = true;
                #endregion


                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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


        #endregion
        public class RouteEmployee : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string RouteUpId { get; set; }
            public string RouteDownId { get; set; }
            public string EmployeeId { get; set; }
            public string PlantId { get; set; }
            public string StopageUpId { get; set; }
            public string StopageDownId { get; set; }
            public string UpDown { get; set; }
            public string RouteId { get; set; }
            public string StopageId { get; set; }
            public string UpDownGrid { get; set; }
            public bool Active { get; set; }

            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class RouteEmployeeList
        {
            public string Id { get; set; }
            public string SystemID { get; set; }
            public string RouteUpGridId { get; set; }
            public string StopageUpGridId { get; set; }
            public string RouteDownGridId { get; set; }
            public string StopageDownGridId { get; set; }

        }
        public class UARouteEmployeeList
        {
            public string Id { get; set; }
            public string EmployeeId { get; set; }
            public string RouteId { get; set; }
            public string StoppageId { get; set; }
            public string ShiftId { get; set; }

        }
        #endregion
    }
}