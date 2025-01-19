using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Model.Taxations;
using Library.Service.Taxations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Aplos.Areas.Employees.Controllers
{
    public class CompensatoryOffNewController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public CompensatoryOffNewController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor


        #region Actions

        public ActionResult Aplos()
        {
            return View();
        }


        [HttpPost]
        public JsonResult Save(CompensatoryOffNew masterdata, List<CompensatoryOffEmpList> employeedata)
        {
            try
            {

                saveData(masterdata, employeedata);




                return Json(new { Message = "Data saved successfully. Day Code:" + masterdata.DayCode, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public JsonResult Delete(string id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("delete FROM [MST].[CompensatoryOff] where id='" + id + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("delete FROM [MST].[CompensatoryOffEmpList] where CompensatoryOffId='" + id + "'", true, "1");

                objCon.CommitTransaction();

                return Json(new { Message = "Data deleted successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("DELETE STATEMENT CONFLICTED WITH THE REFERENCE"))
                    return Json(new { Message = "Employee data exists! cannot delete record. Please clear all employee data first.", Error = true }, JsonRequestBehavior.AllowGet);
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public JsonResult ExcelExport(List<Dictionary<string, string>> data)
        {
            try
            {
                if (data == null)
                    throw new Exception("No data found");

                if (data.Count == 0)
                    throw new Exception("No data found");


                DataTable dt = new DataTable("DD");
                foreach (string item in data[0].Keys)
                {
                    dt.Columns.Add(item);
                }


                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    foreach (string item in data[i].Keys)
                    {
                        dr[item] = data[i][item];
                    }

                    dt.Rows.Add(dr);
                }


                GridToExcelReport(dt);


                return null;
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            //return View();
        }

        [HttpGet, Authorize]
        public ActionResult Download()
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                string fullPath = Path.Combine(HostingEnvironment.MapPath("~") + "autodata.xlsx");
                IWorkbook workbook1 = excelEngine.Excel.Workbooks.Open(fullPath);
                System.IO.File.Delete(fullPath);

                return RenderReportAsExcel(workbook1, "autodata.xlsx");
            }
            catch (Exception)
            {


            }
            return View();
        }

        private void GridToExcelReport(DataTable data)
        {
            try
            { //save the file to server temp folder
                string fullPath = Path.Combine(HostingEnvironment.MapPath("~") + "autodata.xlsx");

                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    IApplication application = excelEngine.Excel;
                    application.DefaultVersion = ExcelVersion.Excel2013;
                    IWorkbook workbook = application.Workbooks.Create(1);
                    IWorksheet sheet = workbook.Worksheets[0];

                    sheet.ImportDataTable(data, true, 2, 1);
                    //sheet.ImportData(data.Select(), 1, 1, true);
                    workbook.SaveAs(fullPath);
                    //workbook.SaveAs("autodata.xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog, ExcelHttpContentType.Excel2013);
                    // return View();
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            finally
            {

            }


        }


        [HttpPost, Authorize]
        public JsonResult GetTaxCategories(string countryid)
        {


            try
            {
                string sql = "select * from [MST].[TaxCategory] where countryid='" + countryid + "' order by sequence";


                return Json(_sqlRepository.GetModelCollection<TaxCategory>(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion Actions
        private void saveData(CompensatoryOffNew masterdata, List<CompensatoryOffEmpList> employeedata)
        {
            string EmpIdLoop = "''";
            if (employeedata != null)
            {

                foreach (CompensatoryOffEmpList item in employeedata)
                {
                    if (EmpIdLoop == "''")
                    {
                        EmpIdLoop = "'" + item.Id + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.Id + "'";

                    }
                }
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
            //obj.LockValidation(identity.PlantId, masterdata.OriginalDate, masterdata.CompensatoryDate,EmpIdLoop);

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsEmployee;
            try
            {
                if (employeedata == null)
                    employeedata = new List<CompensatoryOffEmpList>();

                if (masterdata == null)
                    throw new Exception("No data found");


                if (bplib.clsWebLib.IsDateOK(masterdata.OriginalDate) == false)
                    throw new Exception("Invalid date");


                string sql = "SELECT * FROM [MST].[CompensatoryOff] where OriginalDate='" + masterdata.OriginalDate + "' and plantid='" + identity.PlantId + "' and id<>'" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (string.IsNullOrEmpty(masterdata.Id) == true)
                {
                    //if (dsMaster.Tables[0].Rows.Count > 0)
                    //throw new Exception("This date has already been assigned as working day! [" + masterdata.OriginalDate + "]");
                }


                sql = "SELECT * FROM [MST].[CompensatoryOff] where id='" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                sql = "SELECT * FROM [MST].[CompensatoryOffEmpList] where CompensatoryOffId='" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsEmployee, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    if (bplib.clsWebLib.GetBoolData(dsMaster.Tables[0].Rows[0]["ForEntirePlant"].ToString()) == false)
                        if (masterdata.ForEntirePlant == true)
                            if (dsEmployee.Tables[0].Rows.Count > 0)
                                throw new Exception("Cannot set for entire plant because individual employees are already been tagged");
                }

                DataSet dsTemp;

                if (masterdata.ForEntirePlant == true)
                {
                   
                    //cannot have same start date and transfer date(plant level)
                    sql = @"SELECT * FROM [MST].[CompensatoryOff] M
                            WHERE daytype='" + masterdata.DayType + @"' AND m.OriginalDate='" + masterdata.OriginalDate + @"' AND M.PlantId='" + identity.PlantId + @"' AND m.Id<>'" + masterdata.Id + @"'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsTemp, false, "1");


                    if (dsTemp.Tables[0].Rows.Count > 0)
                        throw new Exception("Plant has already been tagged with date: " + masterdata.OriginalDate + "\r\n");
                }


                //check whether employees exists that day or not
                if (masterdata.ForEntirePlant == false)
                {
                   
                    //cannot have same start date and transfer date (plant level)
                    sql = @"SELECT * FROM [MST].[CompensatoryOff] M
                            WHERE ISNULL(ForEntirePlant,0)=1 AND m.OriginalDate='" + masterdata.OriginalDate + @"' AND M.PlantId='" + identity.PlantId + @"' AND m.Id<>'" + masterdata.Id + @"'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsTemp, false, "1");


                    if (dsTemp.Tables[0].Rows.Count > 0)
                        throw new Exception("Plant has already been tagged with date: " + masterdata.OriginalDate + "\r\n");



                    string faulty = "";
                
                    //cannot have same start date and transfer date (employee)
                    sql = @"SELECT c.EmpSystemId,EI.EmployeeCode FROM [MST].[CompensatoryOff] M
                            INNER JOIN [MST].[CompensatoryOffEmpList] C ON m.Id=c.CompensatoryOffId
                            INNER JOIN EmployeeInformation EI ON EI.SystemID= c.EmpSystemId
                            WHERE m.OriginalDate='" + masterdata.OriginalDate + @"' AND M.PlantId='" + identity.PlantId + @"' AND m.Id<>'" + masterdata.Id + @"'";

                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsTemp, false, "1");


                    faulty = "";
                    for (int i = 0; i < dsTemp.Tables[0].Rows.Count; i++)
                    {
                        IEnumerable<CompensatoryOffEmpList> duplicate = employeedata.Where(ee => ee.Id == dsTemp.Tables[0].Rows[i]["EmpSystemId"].ToString());
                        if (duplicate != null)
                            foreach (CompensatoryOffEmpList item in duplicate)
                                faulty += "[" + dsTemp.Tables[0].Rows[i]["EmployeeCode"].ToString() + "]";
                    }
                    if (faulty != "")
                        throw new Exception("Following employees have already been tagged with date: " + masterdata.OriginalDate + "\r\n" + faulty);

                }



                if (masterdata.ForEntirePlant == true)
                    employeedata = new List<CompensatoryOffEmpList>();

                //get day code
                masterdata.DayCode = "";
                if (masterdata.DayType.ToUpper() == "WORK")
                {
                    masterdata.DayCode += "WA" + masterdata.CompensatoryDateTreatmentType;
                    if (masterdata.IsOriginalDateOTApplicable)
                        masterdata.DayCode += "OT";
                }
                else
                {
                    masterdata.DayCode += "C" + masterdata.CompensatoryDateTreatmentType;
                }



                sql = "SELECT * FROM [MST].[CompensatoryOff] where id='" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                string MasterID = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    string id = "";
                    bplib.clsGenID objID = new bplib.clsGenID();
                    objID.GenID("COMPENSATORY HOLIDAY", out id);

                    MasterID = id;
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["id"] = id;
                    dr["PlantId"] = identity.PlantId;
                    dr["OriginalDate"] = masterdata.OriginalDate;
                    dr["CompensatoryDate"] = masterdata.OriginalDate;
                    dr["CompensatoryDateTreatmentType"] = bplib.clsWebLib.RetValidLen(masterdata.CompensatoryDateTreatmentType);
                    dr["HolidayCategoryId"] = bplib.clsWebLib.RetValidLen(masterdata.HolidayCategoryId);
                    dr["IsOriginalDateOTApplicable"] = masterdata.IsOriginalDateOTApplicable;
                    dr["ForEntirePlant"] = masterdata.ForEntirePlant;
                    dr["DayType"] = masterdata.DayType;
                    dr["DayCode"] = masterdata.DayCode;
                    dr["IsAlignedWithHoliday"] = masterdata.IsAlignedWithHoliday;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);


                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    MasterID = dr["id"].ToString();
                    dr.BeginEdit();

                    dr["OriginalDate"] = masterdata.OriginalDate;
                    dr["CompensatoryDate"] = masterdata.OriginalDate;
                    dr["CompensatoryDateTreatmentType"] = bplib.clsWebLib.RetValidLen(masterdata.CompensatoryDateTreatmentType);
                    dr["HolidayCategoryId"] = bplib.clsWebLib.RetValidLen(masterdata.HolidayCategoryId);
                    dr["IsOriginalDateOTApplicable"] = masterdata.IsOriginalDateOTApplicable;
                    dr["ForEntirePlant"] = masterdata.ForEntirePlant;
                    dr["DayType"] = masterdata.DayType;
                    dr["DayCode"] = masterdata.DayCode;
                    dr["IsAlignedWithHoliday"] = masterdata.IsAlignedWithHoliday;


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                }

                string RowMaster = "''";
                //deleting all unused data
                if (employeedata == null)
                {
                    while (dsEmployee.Tables[0].DefaultView.Count > 0)
                        dsEmployee.Tables[0].DefaultView[0].Delete();
                }
                else
                {
                    for (int i = 0; i < dsEmployee.Tables[0].Rows.Count; i++)
                    {

                        IEnumerable<CompensatoryOffEmpList> filter = employeedata.Where(ee => ee.Id == dsEmployee.Tables[0].Rows[i]["EmpSystemId"].ToString());
                        if (filter == null || filter.Count() == 0)
                        {
                            dsEmployee.Tables[0].Rows[i].Delete();
                        }


                    }


                    string employeeTableID = "";
                    for (int i = 0; i < employeedata.Count; i++)
                    {
                        dsEmployee.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + employeedata[i].Id + "'";
                        if (dsEmployee.Tables[0].DefaultView.Count == 0)
                        {
                            if (employeeTableID == "")
                            {
                                bplib.clsGenID objID = new bplib.clsGenID();
                                objID.GenID("COMPENSATORY HOLIDAY EMPLOYEES", out employeeTableID);
                            }

                            DataRow dr = dsEmployee.Tables[0].NewRow();
                            dr["id"] = employeeTableID + "-" + (i + 1).ToString();

                            string RowId = Convert.ToDateTime(masterdata.OriginalDate).ToString("yyyyMMdd")+ employeedata[i].Id;

                            RowMaster += ",'" + RowId + "'";

                            dr["CompensatoryOffId"] = MasterID;
                            dr["EmpSystemId"] = employeedata[i].Id;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsEmployee.Tables[0].Rows.Add(dr);


                        }

                    }
                }

                clsStaticInfo obj1 = new clsStaticInfo();
                obj1.SaveDataSets(dsMaster, dsEmployee);

                #region Flag Update
                ProcessFlag(RowMaster, masterdata.DayCode);
                #endregion

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }
        public void ProcessFlag(string MainRowId,string DayType)
        {
            try
            {
                var sql = @"update AttdnProcessData set IsManualDayStatus=1,DateUpdated=GetDate(),ManualDayStatus='" + DayType+@"',ManualFlag=1 
                where rowid in ("+MainRowId+ @")";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [HttpPost]
        public JsonResult deleteemployee(CompensatoryOffNew masterdata, string employeedata)
        {
            try
            {
                DataSet dsMaster;
                string sql = " SELECT * FROM [MST].[CompensatoryOffEmpList] WHERE CompensatoryOffId='" + masterdata.Id + "' AND EmpSystemId='" + employeedata + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                    dsMaster.Tables[0].DefaultView[0].Delete();

                clsStaticInfo clsStatic = new clsStaticInfo();
                clsStatic.SaveDataSets(dsMaster);

                return Json(new { Error = false, Message = "Employee Deleted Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public JsonResult LoadCompensatoryOff(string ID)
        {


            try
            {
                string sql = @"SELECT Id, FORMAT( OriginalDate,'dd-MMM-yyyy') AS OriginalDate,FORMAT( CompensatoryDate,'dd-MMM-yyyy') AS CompensatoryDate, CompensatoryDateTreatmentType,
                                HolidayCategoryId,DayType,	DayCode,convert(bit,isnull(IsAlignedWithHoliday,0)) AS IsAlignedWithHoliday, IsOriginalDateOTApplicable, ForEntirePlant FROM [MST].[CompensatoryOff] where ID='" + ID + "'";

                var masterData = _sqlRepository.GetModelCollection<CompensatoryOffNew>(sql);


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sql = @"SELECT Emp.SystemID AS Id,
                                        format(EMP.DOJ,'dd-MMM-yyyy') AS DOJ,
                                        CASE WHEN doj>(CASE WHEN co.OriginalDate<co.CompensatoryDate THEN co.OriginalDate ELSE co.CompensatoryDate END) THEN 'YES' ELSE '' END AS JoinedAfter,

                                case when isnull(O.Id,'')<>'' THEN 1 ELSE 0 END as Active,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                                    EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,
                                        DEPT.UserName Department,S.UserName Section,
                                        PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant
                                        FROM EmployeeInformation EMP
                                        INNER JOIN [MST].[CompensatoryOffEmpList] O ON EMP.SystemID=o.EmpSystemID 
                                        LEFT OUTER JOIN mst.CompensatoryOff AS co ON co.Id=o.CompensatoryOffId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        WHERE emp.PlantID='" + identity.PlantId + @"' AND O.CompensatoryOffId='" + ID + "'";


                var employeeData = _sqlRepository.GetDataCollection(sql);


                return Json(new { master = masterData, employee = employeeData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize, HttpPost]
        public JsonResult HolidayCategory()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                string sql = " SELECT* FROM[SCS].[HolidayCategory] where CompanyGroupID='" + identity.CompanyGroupId + "'";


                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize]
        public JsonResult searchEmployees(string column, string value, string offdate, string CompensatoryDate, bool IsFutureDOJAccepted)
        {
            string strKey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    strKey = column + " like '%" + value + "%'";
                }

            }


            DateTime maxDate = Convert.ToDateTime(offdate);
            //if (Convert.ToDateTime(CompensatoryDate) > maxDate)
            //    maxDate = Convert.ToDateTime(CompensatoryDate);

            //if (Convert.ToDateTime(CompensatoryDate) < Convert.ToDateTime(offdate))
            //    offdate = CompensatoryDate;


            string normalDate = " EMP.EmployeeStatus='Active' ";
            string joinedAfter = "";
            if (Convert.ToDateTime(offdate) < System.DateTime.Now)
            {
                normalDate = " (EMP.DOJ<='" + offdate + "' AND (isnull(dos,'')='' OR DOS>='" + offdate + "')) ";

                if (IsFutureDOJAccepted)
                    normalDate += " OR (DOJ>'" + offdate + "' AND  DOJ<='" + maxDate.ToString("dd-MMM-yyyy") + "') ";

                joinedAfter = "CASE WHEN doj>'" + offdate + "' THEN 'YES' ELSE '' END AS JoinedAfter,";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT Emp.SystemID AS Id,format(EMP.DOJ,'dd-MMM-yyyy') AS DOJ," + joinedAfter + @"
                                --case when isnull(O.Id,'')<>'' THEN 1 ELSE 0 END as Active,
                                    --CASE WHEN isnull(wd.AlignWithCC,0)=1 THEN hrs.DefaultWeekOff ELSE wd.FstOffDay END AS WeekOffDay,
                      CASE WHEN FORMAT(CONVERT(DATETIME,'" + offdate + @"'),'dddd')=CASE WHEN isnull(wd.AlignWithCC,0)=1 THEN hrs.DefaultWeekOff ELSE wd.FstOffDay END 
                      THEN 'YES' ELSE 'NO' END AS WeekOffDay,
                                    convert(bit,0) AS Active,
                                    EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                                    EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,
                                        DEPT.UserName Department,S.UserName Section, emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,
                                        PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant
                                        FROM EmployeeInformation EMP
                                        LEFT OUTER JOIN EmployeeWeekOffByDay WD ON wd.EmpSystemID=emp.SystemId
                                                            AND wd.SystemID=(SELECT TOP 1 SystemID FROM EmployeeWeekOffByDay WHERE EmpSystemID=emp.SystemId AND convert(datetime,EffectiveDate)<='" + offdate + @"' ORDER BY EmployeeWeekOffByDay.EffectiveDate DESC)
                                        LEFT OUTER JOIN PlantWiseHRMSSetting HRS ON hrs.PlantID=emp.PlantId

                                        LEFT OUTER JOIN [MST].[CompensatoryOffEmpList] O ON EMP.SystemID=o.EmpSystemID and CompensatoryOffID=(
                                            select top 1 Id from [MST].[CompensatoryOff] O where PlantID='" + identity.PlantId + @"' and OriginalDate='" + offdate + @"')
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        WHERE emp.PlantID='" + identity.PlantId + @"' and " + normalDate + @") 
                                AS K where " + strKey + " order by EmployeeCodePreFix,EmployeeCodeNumeric";


            try
            {


                var jsondata = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult GetView()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sqlWork = @"SELECT   o.Id,FORMAT( o.OriginalDate,'dd-MMM-yyyy') AS OriginalDate,DayType,DayCode,O.IsAlignedWithHoliday,
                             o.CompensatoryDateTreatmentType,c.UserName AS HolidayCategory,
                            o.IsOriginalDateOTApplicable, o.ForEntirePlant
                             FROM [MST].[CompensatoryOff] O
                            LEFT OUTER JOIN [SCS].[HolidayCategory] C ON c.Id=o.HolidayCategoryId

                            WHERE  O.PlantId='" + identity.PlantId + @"' and DayType='Work'
                            ORDER BY O.OriginalDate DESC";

            string sqlCompensate = @"SELECT   o.Id,FORMAT( o.OriginalDate,'dd-MMM-yyyy') AS OriginalDate,DayType,DayCode,O.IsAlignedWithHoliday,
                             o.CompensatoryDateTreatmentType,c.UserName AS HolidayCategory,
                            o.IsOriginalDateOTApplicable, o.ForEntirePlant
                             FROM [MST].[CompensatoryOff] O
                            LEFT OUTER JOIN [SCS].[HolidayCategory] C ON c.Id=o.HolidayCategoryId

                            WHERE  O.PlantId='" + identity.PlantId + @"' and DayType='Compensate'
                            ORDER BY O.OriginalDate DESC";


            try
            {


                return Json(new { Work = _sqlRepository.GetDataCollection(sqlWork, null), Compensate = _sqlRepository.GetDataCollection(sqlCompensate, null) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
    }

    public class CompensatoryOffNew : BaseModel
    {
        public string Id { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string OriginalDate { get; set; } = "";
        public string CompensatoryDate { get; set; } = "";
        public string CompensatoryDateTreatmentType { get; set; } = "";
        public string HolidayCategoryId { get; set; } = "";
        public bool IsOriginalDateOTApplicable { get; set; } = false;
        public bool ForEntirePlant { get; set; } = false;
        public string DayType { get; set; } = "WORK";//COMPENSATE
        public string DayCode { get; set; } = "";
        public bool IsAlignedWithHoliday { get; set; } = false;
    }

}