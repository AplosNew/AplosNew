using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using OTSBD;
using Library.MaterialManagement.JobWork;
using Library.Model.Enums;
using Syncfusion.XlsIO;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkIssueReturnController : BaseController
    {
        JobWorkIssueReturn JWTIR = new JobWorkIssueReturn();

        string TableName = "dbo.JobWorkIssueReturn";
        string TableName1 = "dbo.JobWorkIssueReturnChild";
        string TableName2 = "JobWorkTransformationIssueReturn";
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkIssueReturnController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
            JWTIR = new JobWorkIssueReturn();
        }
        #endregion
        #region Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Dropdown Code Area

        //[HttpGet, Authorize]
        //public JsonResult gejobworklocation()
        //{
        //    string sql = "";
        //    sql = @"select Id as Value, LocationName as Text from HKP.JobWorkLocation order by LocationName";

        //    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetMaterialCode()
        {
            string sql = "";
            sql = @"select Id as Value, Code as Text  from MST.MaterialMaster order by Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult gearticlecode(string MaterialCodeId)
        {
            string sql = "";
            sql = @"select Id as Value, StandardName as Text from MST.MaterialMasterArticle where MaterialMasterId='"+ MaterialCodeId + "' order by StandardName ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetIndividualReportData(string Id)
        //{
        //    string sql = "";
        //    sql = @"select distinct tir.Id,tc.Id as ContractId, tir.Date, FORMAT(tir.Date,'dd-MMM-yyyy') as IssueDate, tir.ByWhomId, tir.IssueReturn, tir.JobWorkLocationId, tir.Remarks
        //           ,emp.EmployeeName, emp.EmployeeCode, jl.LocationName
        //            from dbo.JobWorkTransformationIssueReturn tir left join dbo.JobWorkTransformationIssueReturnChild tirc on tir.Id=tirc.TransformationIssueReturnMasterId
        //            left join dbo.EmployeeInformation emp on emp.SystemId=tir.ByWhomId
        //            left join HKP.JobWorkLocation jl on jl.Id=tir.JobWorkLocationId
        //            left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tirc.MaterialInputId
        //            left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
        //            left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
        //            where tc.Id='" + Id + @"' order by tir.Date desc ";

        //    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        //}

        #endregion

        #region Load Data

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.JobWorkValueAddedContract where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value, string Type)
        {
            string sql = "";
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if(Type== "ValueAdded")
            {
                sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.VendorPartyId,vac.Remarks,FORMAT(vac.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                           FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                           e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                                           from dbo.JobWorkValueAddedContract vac left join ORG.Entity e on e.Id=vac.EntityId
                left join HKP.Party p on p.Id=vac.VendorPartyId
                                           WHERE " + strkey + " order by ValueAddedDate desc ";

            }
            if(Type == "Transformation")
            {
                sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.VendorPartyId,tc.Remarks,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                                    from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.VendorPartyId
                                    WHERE " + strkey + " order by tc.Date desc";
            }
           
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetDataById(string Id, string TabType)
        {
            string sql = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (TabType == "Value Added")
            {
                sql = @"select vac.Id,TabType='Value Added', vac.EntityId,vac.VendorPartyId,vac.Remarks,FORMAT(vac.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                                    from dbo.JobWorkValueAddedContract vac left join ORG.Entity e on e.Id=vac.EntityId
									left join HKP.Party p on p.Id=vac.VendorPartyId
                                    WHERE vac.Id='"+ Id + "' order by ValueAddedDate desc ";
            }
            if (TabType == "Transformation")
            {
                sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.VendorPartyId,tc.Remarks,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                                    from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.VendorPartyId
                                    WHERE tc.Id='"+ Id + "' order by tc.Date desc";
            }

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetValueAddedChildData(string PKId)
        {
            string sql = "";
 
                sql = @"select distinct vcc.*,vcc.Quantity as VCCQuantity, jwi.UserName as JobWorkItem,jwa.UserName as JobWorkActivity, uom.UserName as OutputUnit, mma.StandardName as ArticleCode, vam.RateApplicable as RateApply, c.Code as Currency, emp.EmployeeName as ResponsiblePerson
                               ,owr.Id as OWRId, owr.JobWorkValueAddedContractChildMasterId, owr.OrderType,owr.Quantity as OWRQuantity,owr.PlanQuantity
							   ,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, owruom.UserName as OWRUOM
							   ,IssueQuantity=case WHEN vcc.OrderSpecific = 'Yes' THEN (kk.TotalQuantity) ELSE (TQ.TQuantity) END
							   ,BalToIssue=case WHEN vcc.OrderSpecific = 'Yes' THEN (owr.Quantity-kk.TotalQuantity) WHEN vcc.OrderSpecific = 'NO' THEN (vcc.Quantity-TQ.TQuantity) ELSE '0' END
                               ,IssueActive='Active'
                               from dbo.JobWorkValueAddedContractChild vcc left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
							   left join hkp.JobWorkActivity jwa on jwa.Id=vcc.JobActivityId
        					   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleCodeId
        					   left join MST.JobWorkValueAddedMaster vam on vam.Id=vcc.RateApplyId
        					   left join scs.Currency c on c.Id=vcc.CurrencyId and vcc.CurrencyId=vam.CurrencyId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
							   left join dbo.JobWorkValueAddedContract vc on vc.Id=vcc.JobWorkValueAddedContractMasterId
							   left join dbo.JobWorkValueAddedContractChild2 owr on owr.JobWorkValueAddedContractChildMasterId=vcc.Id
							   left join HKP.Party P on P.Id=owr.CustomerId
							   left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
        					   left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
        			    		left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
        				    	left join SCS.UnitOfMeasurement owruom on owruom.Id=owr.OutputMaterialUOMId
								left join (	select SUM(quantity) as TotalQuantity,ContractLineItemId,OrderChildId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId,OrderChildId
										) kk on kk.ContractLineItemId=vcc.Id and kk.OrderChildId=owr.Id
								left join (	select SUM(quantity) as TQuantity,ContractLineItemId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId
										) TQ on TQ.ContractLineItemId=vcc.Id
        					   where vc.Id='" + PKId + "' ";
            

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTransformationChildData(string PKId)
        {
            string sql = "";

            sql = @"select distinct mp.*, jwi.UserName as JobWorkItem,jwa.UserName as JobWorkActivity, uom.UserName as OutputUnit, mma.StandardName as ArticleCode, tm.RateApplicable as RateApply
                               ,c.Code as Currency, emp.EmployeeName as ResponsiblePerson, JL.LocationName as MaterialLocation
                               from dbo.JobWorkTransformationContractChild mp left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
        					   left join SCS.UnitOfMeasurement uom on uom.Id=mp.OutputMaterialUOMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
        					   left join MST.JobWorkTransformationMaster tm on tm.Id=mp.RateApplyId
							   left join scs.Currency c on c.Id=mp.CurrencyId and mp.CurrencyId=tm.CurrencyId
							   left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=mp.ResponsiblePersonId
							   left join HKP.JobWorkLocation JL on JL.Id=mp.MaterialLocationId
					   	   	  left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
        					   where tc.Id='" + PKId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(JWTIR.GetMaterialInputData(SelectedMaterialPlanningData), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetLotNoRate(string LotNumber)
        {
            try
            {
  
                return Json(JWTIR.GetLotNoRate(LotNumber), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                           EMP.EmployeeName,EMP.EmployeeCode AS Code,
                           EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                               PR.UserName PositionName,
                               DEPT.UserName DepartmentName,S.UserName Section,
                               EMP.SectionId,SS.UserName SubSection
                               ,PL.UserName Plant
                               FROM EmployeeInformation EMP
                               LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                               LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                               LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                               LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                               LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                               LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                               LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                               LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                               LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                           WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                      AND isnull(Emp.SystemID,'') not in (select isnull(ByWhomId,'') from dbo.JobWorkIssueReturn where Id='" + Id + @"')
                     order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }



        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkIssueReturn", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "I" + GetPK();
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

        #endregion

        //   // Child data

        private string GetIssueChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkIssueReturnChild", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult SaveIssueChild(IEnumerable<JobWorkIssueReturnChild> IssueChildTabData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;
     
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var MPId = "' '";
                var OWRId = "''";
                foreach (var empitem in IssueChildTabData)
                {
                    MPId += ",'" + empitem.Id + "' ";
                    OWRId += ",'" + empitem.OWRId + "' ";
                }
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where (ContractLineItemId IN ( " + MPId + " ) or OrderChildId IN (" + OWRId + ")) and JobWorkIssueReturnMasterId='"+ MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in IssueChildTabData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "IC" + GetIssueChildPK();

                        dr["JobWorkIssueReturnMasterId"] = MasterId;

                        dr["ContractLineItemId"] = item.Id;
                        dr["OrderChildId"] = item.OWRId;
                        dr["Quantity"] = item.BalToIssue;

                        dr["Remarks"] = item.Remarks;
                        dr["Active"] = item.IssueActive;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        #region Reports for Value Added Contract

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpGet, Authorize]
        public ActionResult GetValueAddedPrintReport(ReportFormat reportFormat, string PrintTabId, string IssueId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Value Added Job Work Material Issue Chalaan " + PrintTabId + "";
            var workbook = GetContractReportWorkSheet(PrintTabId, IssueId);
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

        private IWorkbook GetContractReportWorkSheet(string PrintTabId, string IssueId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "ValueAddedContractIssueChalaan";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetContractReportDataById(PrintTabId, IssueId);
            DataTable IssueReturnChilddata = GetIssueReturnChildDataById(PrintTabId);
            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
            //    int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Date", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["ValueAddedDate"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                ColEntityHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;

               

                int ColIssueIdEnd = ColEntityEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Issue Id", 20, ExcelHAlign.HAlignLeft);
                ColIssueIdEnd++;
                int ColVAProcessEndDate = ColIssueIdEnd;
                int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["IssueId"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Issue Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColIssueDate = ColVAProcessEndDateEnd;
                int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["IssueDate"].ToString();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Merge();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //    ColIssueDateEnd++;

                int ColPStartDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColPStartDate, "Process Start Date", 12, ExcelHAlign.HAlignLeft);
                ColPStartDate++;
                ColVAProcessStartDateEnd = ColPStartDate + 1;
                int ColAddress = ColPStartDate;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["VAProcessStartDate"].ToString();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

           //     int ColPEndDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Process End Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColProcessEndDate = ColVAProcessStartDateEnd;
                int ColProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Text = data.Rows[0]["VAProcessEndDate"].ToString();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColProcessEndDateEnd++;

                int ColPrtyName = ColProcessEndDateEnd+1;
                SetHeaderTextTop(ref sheet, ROW, ColPrtyName, "Party Name", 20, ExcelHAlign.HAlignLeft);
                ColPrtyName++;
                int ColPartyName = ColPrtyName;
                int ColPartyNameEnd = ColPrtyName + 1;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Text = data.Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //     ROW++;
                ColPartyNameEnd++;

              
                int ColIssuebyEnd = ColPartyNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Issue By", 20, ExcelHAlign.HAlignLeft);
                ColIssuebyEnd++;
                int ColIssueby = ColIssuebyEnd;
                int ColIssueByEnd = ColIssuebyEnd + 1;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Merge();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //  ColIssueByEnd++;

                int ColCCDATe = 1;
                SetHeaderTextTop(ref sheet, ROW, ColCCDATe, "Contract Closing Date", 20, ExcelHAlign.HAlignLeft);
                ColCCDATe++;
                int ColVAContractClosingDate = ColCCDATe;
                int ColVAContractClosingDateEnd = ColCCDATe + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["VAContractClosingDate"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "User Contract Reference", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColUserContractReference = ColVAContractClosingDateEnd;
                int ColUserContractReferenceEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].Text = data.Rows[0]["UserContractReference"].ToString();
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].Merge();
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColUserContractReferenceEnd++;


                int ColIR = ColUserContractReferenceEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIR, "Issue Type/ Issue Return", 15, ExcelHAlign.HAlignLeft);
                ColIR++;
                int ColIssueReturn = ColIR;
                int ColIssueReturnEnd = ColIR + 1;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["IssueReturn"].ToString();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColIssueReturnEnd++;



                SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Issue Location", 20, ExcelHAlign.HAlignLeft);
                ColIssueReturnEnd++;
                int ColJobWorkLocation = ColIssueReturnEnd;
                int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["JobWorkLocation"].ToString();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                int ColRemarks = 1;
                SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColRemarks++;
                int ColContractRemarks = ColRemarks;
                int ColContractRemarksEnd = ColRemarks + 1;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
       

            }

     //       Issue/ Return Child data

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue/ Return Quantity", 12, ExcelHAlign.HAlignLeft);
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Job Work Item", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material Type", 8, ExcelHAlign.HAlignLeft);
            int ColMaterialType = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Article Code", 12, ExcelHAlign.HAlignLeft);
            int ColArticleCode = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Output Unit", 8, ExcelHAlign.HAlignLeft);
            int ColOutputUnit = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColVCCQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Specific", 8, ExcelHAlign.HAlignLeft);
            int ColOrderSpecific = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Wise Quantity", 8, ExcelHAlign.HAlignLeft);
            int ColOWRQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Master Order No", 12, ExcelHAlign.HAlignLeft);
            int ColMasterOrderNo = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material Order Item", 10, ExcelHAlign.HAlignLeft);
            int ColMaterialOrderItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Customer", 8, ExcelHAlign.HAlignLeft);
            int ColCustomer = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Plan Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColPlanQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue Quantity", 8, ExcelHAlign.HAlignLeft);
            int ColIssueQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Balance To Issue", 8, ExcelHAlign.HAlignLeft);
            int ColBalToIssue = MPChildCOL;
       //     MPChildCOL++;

            //report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            //int ColMPCRemarks = MPChildCOL;
            MPChildROW++;
            MPChildendCol = MPChildCOL;
            #endregion Headers

            string JobWorkItem = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < IssueReturnChilddata.Rows.Count; i++)
            {

                if (JobWorkItem != IssueReturnChilddata.Rows[i]["JobWorkItem"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColMaterialType].Text = IssueReturnChilddata.Rows[i]["MaterialType"].ToString();
                sheet[MPChildROW, ColJobWorkItem].Text = IssueReturnChilddata.Rows[i]["JobWorkItem"].ToString();
                sheet[MPChildROW, ColArticleCode].Text = IssueReturnChilddata.Rows[i]["ArticleCode"].ToString();
                sheet[MPChildROW, ColOutputUnit].Text = IssueReturnChilddata.Rows[i]["OutputUnit"].ToString();
                sheet[MPChildROW, ColVCCQuantity].Number = clsStaticInfo.dbl(IssueReturnChilddata.Rows[i]["VCCQuantity"].ToString());
                sheet[MPChildROW, ColOrderSpecific].Text = IssueReturnChilddata.Rows[i]["OrderSpecific"].ToString();
                sheet[MPChildROW, ColOWRQuantity].Number = clsStaticInfo.dbl(IssueReturnChilddata.Rows[i]["OWRQuantity"].ToString());
                sheet[MPChildROW, ColPlanQuantity].Number = clsStaticInfo.dbl(IssueReturnChilddata.Rows[i]["PlanQuantity"].ToString());
                sheet[MPChildROW, ColCustomer].Text = IssueReturnChilddata.Rows[i]["Customer"].ToString();
        //        sheet[MPChildROW, ColMPCRemarks].Text = IssueReturnChilddata.Rows[i]["Remarks"].ToString();
                sheet[MPChildROW, ColMasterOrderNo].Text = IssueReturnChilddata.Rows[i]["MasterOrderNo"].ToString();
                sheet[MPChildROW, ColMaterialOrderItem].Text = IssueReturnChilddata.Rows[i]["MaterialOrderItem"].ToString();
                sheet[MPChildROW, ColIssueQuantity].Text = IssueReturnChilddata.Rows[i]["IssueQuantity"].ToString();
                sheet[MPChildROW, ColBalToIssue].Text = IssueReturnChilddata.Rows[i]["BalToIssue"].ToString();

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                JobWorkItem = IssueReturnChilddata.Rows[i]["JobWorkItem"].ToString();

                MPChildROW++;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, MPChildendCol, "Value Added Job Work Material Issue Chalaan", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetContractReportDataById(string PrintTabId, string IssueId)
        {
            var sql = @"select distinct vac.*,TabType='Value Added',FORMAT(vac.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,ir.Id as IssueId,FORMAT(ir.Date,'dd-MMM-yyyy') as IssueDate,emp.EmployeeName as ByWhom
									,JL.LocationName as JobWorkLocation,ir.IssueReturn
                                    from dbo.JobWorkValueAddedContract vac left join ORG.Entity e on e.Id=vac.EntityId
									left join HKP.Party p on p.Id=vac.VendorPartyId
									left join dbo.JobWorkValueAddedContractChild mp on mp.JobWorkValueAddedContractMasterId=vac.Id
									left join dbo.JobWorkValueAddedContractChild2 owr on owr.JobWorkValueAddedContractChildMasterId=mp.Id
									left join dbo.JobWorkIssueReturnChild irc on irc.ContractLineItemId=mp.Id
									left join dbo.JobWorkIssueReturnChild on irc.OrderChildId=owr.Id
									left join dbo.JobWorkIssueReturn ir on ir.Id=irc.JobWorkIssueReturnMasterId
									left join dbo.EmployeeInformation emp on emp.SystemId=ir.ByWhomId
									left join HKP.JobWorkLocation JL on JL.Id=ir.JobWorkLocationId
                                    where vac.Id = '" + PrintTabId + "' and ir.Id='"+ IssueId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetIssueReturnChildDataById(string PrintTabId)
        {
            var sql = @"select distinct vcc.*,vcc.Quantity as VCCQuantity, jwi.UserName as JobWorkItem, uom.UserName as OutputUnit, mma.StandardName as ArticleCode, vam.RateApplicable as RateApply, c.Code as Currency, emp.EmployeeName as ResponsiblePerson
                               ,owr.Id as OWRId, owr.JobWorkValueAddedContractChildMasterId, owr.OrderType,owr.Quantity as OWRQuantity,owr.PlanQuantity
							   ,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, owruom.UserName as OWRUOM
							   ,IssueQuantity=case WHEN vcc.OrderSpecific = 'Yes' THEN (kk.TotalQuantity) ELSE (TQ.TQuantity) END
							   ,BalToIssue=case WHEN vcc.OrderSpecific = 'Yes' THEN (owr.Quantity-kk.TotalQuantity) WHEN vcc.OrderSpecific = 'NO' THEN (vcc.Quantity-TQ.TQuantity) ELSE '0' END
                               ,IssueActive='Active'
                               from dbo.JobWorkValueAddedContractChild vcc left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
        					   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
        					   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleCodeId
        					   left join MST.JobWorkValueAddedMaster vam on vam.Id=vcc.RateApplyId
        					   left join scs.Currency c on c.Id=vcc.CurrencyId and vcc.CurrencyId=vam.CurrencyId
        					   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
							   left join dbo.JobWorkValueAddedContract vc on vc.Id=vcc.JobWorkValueAddedContractMasterId
							   left join dbo.JobWorkValueAddedContractChild2 owr on owr.JobWorkValueAddedContractChildMasterId=vcc.Id
							   left join HKP.Party P on P.Id=owr.CustomerId
							   left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
        					   left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
        			    		left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
        				    	left join SCS.UnitOfMeasurement owruom on owruom.Id=owr.OutputMaterialUOMId
								left join (	select SUM(quantity) as TotalQuantity,ContractLineItemId,OrderChildId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId,OrderChildId
										) kk on kk.ContractLineItemId=vcc.Id and kk.OrderChildId=owr.Id
								left join (	select SUM(quantity) as TQuantity,ContractLineItemId FROM dbo.JobWorkIssueReturnChild group by ContractLineItemId
										) TQ on TQ.ContractLineItemId=vcc.Id
        					   where vc.Id='" + PrintTabId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Value Added Contract

        // TRANSFORMATION ISSUE

        [HttpPost, Authorize]
        public ActionResult LoadAllResponsiblePersonDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                           EMP.EmployeeName,EMP.EmployeeCode AS Code,
                           EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                               PR.UserName PositionName,
                               DEPT.UserName DepartmentName,S.UserName Section,
                               EMP.SectionId,SS.UserName SubSection
                               ,PL.UserName Plant
                               FROM EmployeeInformation EMP
                               LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                               LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                               LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                               LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                               LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                               LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                               LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                               LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                               LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                           WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                      AND isnull(Emp.SystemID,'') not in (select isnull(ByWhomId,'') from dbo.JobWorkTransformationIssueReturn where Id='" + Id + @"')
                     order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize, HttpGet]
        public JsonResult getentitylist()
        {
            try
            {

                return Json(JWTIR.getentitylist(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult gejobworklocation()
        {
            try
            {

                return Json(JWTIR.gejobworklocation(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //private string GetTransformationPK()
        //{
        //    string sID = string.Empty;
        //    bplib.clsGenID objGenID = new bplib.clsGenID();
        //    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkTransformationIssueReturn", out sID);
        //    return sID;
        //}

        //[HttpPost]
        //public JsonResult SaveIssueTransformation(Dictionary<string, object> data)
        //{
        //    try
        //    {
        //        DataSet dsMaster;
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

        //        con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

        //        string _Id = "";

        //        #region data update
        //        if (dsMaster.Tables[0].Rows.Count == 0)
        //        {
        //            bplib.clsGenID genid = new bplib.clsGenID();

        //            genid.GenID(TableName2, out _Id);

        //            data["Id"] = "IT" + GetTransformationPK();
        //            AddNewRow(dsMaster.Tables[0], data);
        //        }
        //        else
        //        {
        //            _Id = data["Id"].ToString();
        //            EditRow(dsMaster.Tables[0].Rows[0], data);
        //        }
        //        #endregion data update

        //        clsStaticInfo _info = new clsStaticInfo();
        //        _info.SaveDataSets(dsMaster);

        //        return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

        //    }
        //    catch (Exception ex)
        //    {

        //        return Json(new { Error = true, Message = ex.Message });

        //    }
        //}

        [HttpPost]
        public JsonResult SaveIssueTransformation(Dictionary<string, object> data, string ContractId, string ContractType)
        {
            try
            {
                JWTIR.SaveIssueTransformation(data, ContractId, ContractType);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        //   // Child data

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkTransformationIssueReturnChild", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult SaveTransformationChild(IEnumerable<JobWorkTransformationIssueReturnChild> SelectedQuantityData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var MatInputId = "' '";
            
                foreach (var empitem in SelectedQuantityData)
                {
                    MatInputId += ",'" + empitem.Id + "' ";
                 
                }
                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkTransformationIssueReturnChild where MaterialInputId IN ( " + MatInputId + ") and TransformationIssueReturnMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in SelectedQuantityData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "TC" + GetTransformationChildPK();

                        dr["TransformationIssueReturnMasterId"] = MasterId;

                        dr["MaterialInputId"] = item.Id;
                        dr["MaterialMasterId"] = item.InputMaterialId;
                        dr["Quantity"] = item.Quantity;
                        dr["Remarks"] = item.Remarks;
                        dr["MaterialMasterArticleId"] = item.MaterialMasterArticleId;
                        dr["Value"] = item.Value;
                        dr["LotNumber"] = item.LotNumber;
                     
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult LoadAllMaterialMstDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select mm.Id, mm.Code, mm.UserName as MaterialName,mc.UserName as MaterialCategory, mgm.UserName as MaterialGroupMaster, buom.UserName as BaseUOM
                                      ,WithSKU=case when mm.WithSKU=0 then 'No' else 'Yes' END
									  ,IsAsset=case when mm.IsAsset=0 then 'No' else 'Yes' END
                                      from MST.MaterialMaster mm left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
									  left join SCS.UnitOfMeasurement buom on buom.Id=mm.BaseUOMId
									  left join HKP.MaterialCategory mc on mc.Id=mm.MaterialCategoryId
                                      WHERE mm.CompanyGroupId='" + identity.CompanyGroupId + @"' order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllMaterialMstArticle(string MaterialMstId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"Select mm.Code as MaterialCode,mm.UserName as Material,mgm.UserName as MaterialGroupMaster,mm.Id MaterialMasterId,mma.Id as ArticleId ,mma.Code as ArticleCode, mma.ShortName, mma.StandardName 
                           from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id
                           left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
                            where mm.Id='" + MaterialMstId + @"' order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetByDefaultRate(string ArticleId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select im.MaterialMasterId, im.ArticleId,SUM(ird.Rate) as Rate 
                           from TRN.InventoryMaterial im
                           left join (Select InventoryMaterialId,(sum( MaterialTranAmount)/sum(TransactionQty)) as Rate from TRN.InventoryReceiveDetail group by InventoryMaterialId)
                           ird on ird.InventoryMaterialId=im.Id
                           where im.ArticleId='"+ ArticleId + @"' and im.PlantId='" + identity.PlantId + @"'
                           group by im.MaterialMasterId, im.ArticleId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLotNumberList(string ArticleId, string MaterialId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select distinct IRD.LotNo Value, IRD.LotNo Text, IM.MaterialMasterId, IM.ArticleId from trn.InventoryReceiveDetail IRD
                                      left join trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                                      where PlantId='"+ identity.PlantId + @"' and IM.MaterialMasterId='"+ MaterialId + @"' and IM.ArticleId='"+ ArticleId + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        // REPORT FOR TRANSFORMATION ISSUE

        #region Reports for Transformation Contract

        [HttpGet, Authorize]
        public ActionResult GetTransformationPrintReport(ReportFormat reportFormat, string PrintTabId, string IssueId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Transformation Job Work Material Issue Chalaan " + PrintTabId + "";
            var workbook = GetTransformationContractReportWorkSheet(PrintTabId, IssueId);
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

        private IWorkbook GetTransformationContractReportWorkSheet(string PrintTabId, string IssueId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];

            sheet.Name = "TransformationContractIssueChalaan";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetTransformationContractReportDataById(PrintTabId, IssueId);
            DataTable TransformationIssueReturnChilddata = GetTransformationIssueReturnChildDataById(IssueId);
            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
                //    int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Date", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["ValueAddedDate"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                ColEntityHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;



                int ColIssueIdEnd = ColEntityEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIssueIdEnd, "Issue Id", 20, ExcelHAlign.HAlignLeft);
                ColIssueIdEnd++;
                int ColVAProcessEndDate = ColIssueIdEnd;
                int ColVAProcessEndDateEnd = ColIssueIdEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["TransformationIssueId"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Issue Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColIssueDate = ColVAProcessEndDateEnd;
                int ColIssueDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Text = data.Rows[0]["TransformationDate"].ToString();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].Merge();
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueDate, ROW, ColIssueDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //    ColIssueDateEnd++;

                int ColPStartDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColPStartDate, "Process Start Date", 12, ExcelHAlign.HAlignLeft);
                ColPStartDate++;
                ColVAProcessStartDateEnd = ColPStartDate + 1;
                int ColAddress = ColPStartDate;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["VAProcessStartDate"].ToString();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPStartDate, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

                //     int ColPEndDate = 1;
                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Process End Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColProcessEndDate = ColVAProcessStartDateEnd;
                int ColProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Text = data.Rows[0]["VAProcessEndDate"].ToString();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColProcessEndDate, ROW, ColProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColProcessEndDateEnd++;

                int ColPrtyName = ColProcessEndDateEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColPrtyName, "Party Name", 20, ExcelHAlign.HAlignLeft);
                ColPrtyName++;
                int ColPartyName = ColPrtyName;
                int ColPartyNameEnd = ColPrtyName + 1;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Text = data.Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //     ROW++;
                ColPartyNameEnd++;


                int ColIssuebyEnd = ColPartyNameEnd;
                SetHeaderTextTop(ref sheet, ROW, ColIssuebyEnd, "Issue By", 20, ExcelHAlign.HAlignLeft);
                ColIssuebyEnd++;
                int ColIssueby = ColIssuebyEnd;
                int ColIssueByEnd = ColIssuebyEnd + 1;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Text = data.Rows[0]["ByWhom"].ToString();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].Merge();
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueby, ROW, ColIssueByEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //  ColIssueByEnd++;

                int ColCCDATe = 1;
                SetHeaderTextTop(ref sheet, ROW, ColCCDATe, "Contract Closing Date", 20, ExcelHAlign.HAlignLeft);
                ColCCDATe++;
                int ColVAContractClosingDate = ColCCDATe;
                int ColVAContractClosingDateEnd = ColCCDATe + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["VAContractClosingDate"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "Contract Id", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColContractId = ColVAContractClosingDateEnd;
                int ColContractIdEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].Text = data.Rows[0]["Id"].ToString();
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].Merge();
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractId, ROW, ColContractIdEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColContractIdEnd++;


                int ColIR = ColContractIdEnd + 1;
                SetHeaderTextTop(ref sheet, ROW, ColIR, "Issue Type/ Issue Return", 15, ExcelHAlign.HAlignLeft);
                ColIR++;
                int ColIssueReturn = ColIR;
                int ColIssueReturnEnd = ColIR + 1;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Text = data.Rows[0]["IssueReturn"].ToString();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].Merge();
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueReturn, ROW, ColIssueReturnEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColIssueReturnEnd++;



                SetHeaderTextTop(ref sheet, ROW, ColIssueReturnEnd, "Issue Location", 20, ExcelHAlign.HAlignLeft);
                ColIssueReturnEnd++;
                int ColJobWorkLocation = ColIssueReturnEnd;
                int ColJobWorkLocationEnd = ColIssueReturnEnd + 1;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Text = data.Rows[0]["JobWorkLocation"].ToString();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].Merge();
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColJobWorkLocation, ROW, ColJobWorkLocationEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

                int ColRemarks = 1;
                SetHeaderTextTop(ref sheet, ROW, ColRemarks, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColRemarks++;
                int ColContractRemarks = ColRemarks;
                int ColContractRemarksEnd = ColRemarks + 1;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].Merge();
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColContractRemarks, ROW, ColContractRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColContractRemarksEnd++;

                int ColContractIsseStatus = ColContractRemarksEnd + 4;
                SetHeaderTextTop(ref sheet, ROW, ColContractIsseStatus, "Issue Status", 20, ExcelHAlign.HAlignLeft);
                ColContractIsseStatus++;
                int ColIssueStatus = ColContractIsseStatus;
                int ColIssueStatusEnd = ColContractIsseStatus + 1;
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Text = data.Rows[0]["IssueStatus"].ToString();
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].Merge();
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColIssueStatus, ROW, ColIssueStatusEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;


            }

            //       Issue/ Return Child data

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issue/ Return Quantity", 12, ExcelHAlign.HAlignLeft);
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItemId = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Output Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWOutputItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Input Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputItemId = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Input Material", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputMaterial = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "JW Input Article", 12, ExcelHAlign.HAlignLeft);
            int ColArticle = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Required Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColRequiredQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Balance To Issue", 12, ExcelHAlign.HAlignLeft);
            int ColBalanceToIssue = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Total Issued Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTIRCTotalQty = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Issued Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColTIRCQty = MPChildCOL;
            MPChildROW++;
            MPChildendCol = MPChildCOL;
            #endregion Headers

            string JWOutputItem = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < TransformationIssueReturnChilddata.Rows.Count; i++)
            {

                if (JWOutputItem != TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColJWOutputItemId].Text = TransformationIssueReturnChilddata.Rows[i]["JobWorkTransformationContractChildMasterId"].ToString();
                sheet[MPChildROW, ColJWOutputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();
                sheet[MPChildROW, ColJWInputItemId].Text = TransformationIssueReturnChilddata.Rows[i]["Id"].ToString();
                sheet[MPChildROW, ColJWInputItem].Text = TransformationIssueReturnChilddata.Rows[i]["JWInputItem"].ToString();
                sheet[MPChildROW, ColJWInputMaterial].Text = TransformationIssueReturnChilddata.Rows[i]["JWInputMaterial"].ToString();
                sheet[MPChildROW, ColArticle].Text = TransformationIssueReturnChilddata.Rows[i]["JWInputArticle"].ToString();
                sheet[MPChildROW, ColBalanceToIssue].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["BalanceToIssue"].ToString());
                sheet[MPChildROW, ColRequiredQuantity].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["RequiredQuantity"].ToString());
                sheet[MPChildROW, ColTIRCTotalQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TIRCTotalQty"].ToString());
                sheet[MPChildROW, ColTIRCQty].Number = clsStaticInfo.dbl(TransformationIssueReturnChilddata.Rows[i]["TIRCQty"].ToString());

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                JWOutputItem = TransformationIssueReturnChilddata.Rows[i]["JWOutputItem"].ToString();

                MPChildROW++;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColJobWorkItem, MPChildROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColJWOutputItem, MPChildROW - 1, ColJWOutputItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, MPChildendCol+6, "Transformation Job Work Material Issue Chalaan", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetTransformationContractReportDataById(string PrintTabId, string IssueId)
        {
            var sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.VendorPartyId,tc.Remarks,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
									,ti.Id as TransformationIssueId,FORMAT(ti.Date,'dd-MMM-yyyy') as TransformationDate,emp.EmployeeName as ByWhom
									,JL.LocationName as JobWorkLocation,ti.IssueReturn
									,IssueStatus=case when ti.IsConfirmed=0 then 'Not Confirmed' else 'Confirmed' End
                                    from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.VendorPartyId
									left join dbo.JobWorkTransformationContractChild mp on tc.Id=mp.JobWorkTransformationContractMasterId
									left join dbo.JobWorkTransformationContractChild3 mi on mp.Id=mi.JobWorkTransformationContractChildMasterId
									left join dbo.JobWorkTransformationIssueReturnChild tic on mi.Id=tic.MaterialInputId
									left join dbo.JobWorkTransformationIssueReturn ti on ti.Id=tic.TransformationIssueReturnMasterId
									left join dbo.EmployeeInformation emp on emp.SystemId=ti.ByWhomId
									left join HKP.JobWorkLocation JL on JL.Id=ti.JobWorkLocationId
                                    WHERE tc.Id='" + PrintTabId + "' and ti.Id='"+ IssueId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetTransformationIssueReturnChildDataById(string IssueId)
        {
            var sql = @"select distinct mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem ,mm.Id as JWInputMaterialMasterId
                            , mm.UserName as JWInputMaterial ,mma.Id as JWInputMaterialArticleId, mma.StandardName as JWInputArticle
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                            ,SUM(tirc.Quantity) as TIRCQty
                            ,Sum(kk.TotalQuantity) as TIRCTotalQty
                             from dbo.JobWorkTransformationIssueReturnChild tirc left join dbo.JobWorkTransformationContractChild3 mi on tirc.MaterialInputId=mi.Id
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join MST.MaterialMasterArticle mma on mma.Id=tirc.MaterialMasterArticleId
							 left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=tirc.MaterialMasterId
                             left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join(select SUM(Quantity) as TotalQuantity,MaterialInputId FROM dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId) kk on kk.MaterialInputId=mi.id
                             left join TRN.InventoryMaterial inm on inm.MaterialMasterId=mm.Id and inm.ArticleId=mma.Id
                             left join (Select InventoryMaterialId,(sum( MaterialTranAmount)/sum(TransactionQty)) as Rate from TRN.InventoryReceiveDetail group by InventoryMaterialId) InvDetail on InvDetail.InventoryMaterialId=inm.Id
							 where tirc.TransformationIssueReturnMasterId='"+ IssueId + @"' 
							 group by mi.Id, mm.Id, mm.UserName,InvDetail.Rate ,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity, InvDetail.InventoryMaterialId,mi.JobWorkTransformationContractChildMasterId,jwi.UserName,jwii.UserName ";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Transformation Contract

        // New Changes

        [Authorize, HttpGet]
        public JsonResult GetCostCenterLoadNewFun(string EntityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(JWTIR.GetCostCenterLoadNewFun(EntityId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [Authorize, HttpGet]
        public JsonResult GetDataByInventoryIssue(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(JWTIR.GetDataByInventoryIssue(Id, identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }
}
public class JobWorkIssueReturnChild
{

    #region Scalar Properties

    public string Id { get; set; }
    public string JobWorkIssueReturnMasterId { get; set; }
    public string ContractLineItemId { get; set; }
    public string OrderChildId { get; set; }
    public string Quantity { get; set; }
    public string Remarks { get; set; }
    public string OWRId { get; set; }
    public string OrderSpecific { get; set; }
    public string IssueQuantity { get; set; }
    public string BalToIssue { get; set; }
    public string IssueActive { get; set; }


    #endregion Scalar Properties
}

public class JobWorkTransformationIssueReturnChild
{

    #region Scalar Properties

    public string Id { get; set; }
    public string Material { get; set; }
    public string Article { get; set; }
    public string InputMaterialId { get; set; }
    public string MaterialMasterArticleId { get; set; }
    public string Quantity { get; set; }
    public string Remarks { get; set; }
    public string Value { get; set; }
    public string LotNumber { get; set; }


    #endregion Scalar Properties
}