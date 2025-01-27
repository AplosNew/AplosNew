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
    public class LayOffController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public LayOffController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor


        #region Actions
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [Authorize, HttpPost]
        public JsonResult Save(LayOff masterdata, List<LayOffEmpList> employeedata)
        {
            try
            {

                saveData(masterdata, employeedata);




                return Json(new { Message = "Data saved successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        [Authorize, HttpPost]
        public JsonResult Delete(string id)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("delete FROM [MST].[LayOffEmpList] where LayOffId='" + id + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("delete FROM [MST].[LayOff] where id='" + id + "'", true, "1");

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

        [HttpPost]
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

        [HttpGet]
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


        [HttpPost]
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
        private void saveData(LayOff masterdata, List<LayOffEmpList> employeedata)
        {
            string EmpIdLoop = "''";
            if (employeedata != null)
            {

                foreach (LayOffEmpList item in employeedata)
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

            clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
            obj.LockValidation(identity.PlantId, masterdata.FromDate, masterdata.ToDate, EmpIdLoop);

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsEmployee;
            try
            {
                if (employeedata == null)
                    employeedata = new List<LayOffEmpList>();

                if (masterdata == null)
                    throw new Exception("No data found");


                if (bplib.clsWebLib.IsDateOK(masterdata.FromDate) == false)
                    throw new Exception("Invalid from date");

                if (bplib.clsWebLib.IsDateOK(masterdata.ToDate) == false)
                    throw new Exception("Invalid to date");

                if (Convert.ToDateTime(masterdata.ToDate) < Convert.ToDateTime(masterdata.FromDate))
                    throw new Exception("to Date cannot be earlier than from date");



                DataSet dsTemp;
                //cannot have same start date and transfer date (employee)
                string sql = @"SELECT distinct c.EmpSystemId,EI.EmployeeCode FROM [MST].[LayOff] M
                            INNER JOIN [MST].[LayOffEmpList] C ON m.Id=c.LayOffId
                            INNER JOIN EmployeeInformation EI ON EI.SystemID= c.EmpSystemId
                            WHERE C.WorkDate between '" + masterdata.FromDate + @"' AND '" + masterdata.ToDate + @"' AND M.PlantId='" + identity.PlantId + @"' AND m.Id<>'" + masterdata.Id + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsTemp, false, "1");


                string faulty = "";
                for (int i = 0; i < dsTemp.Tables[0].Rows.Count; i++)
                {
                    IEnumerable<LayOffEmpList> duplicate = employeedata.Where(ee => ee.Id == dsTemp.Tables[0].Rows[i]["EmpSystemId"].ToString());
                    if (duplicate != null)
                        foreach (LayOffEmpList item in duplicate)
                            faulty += "[" + dsTemp.Tables[0].Rows[i]["EmployeeCode"].ToString() + "]";
                }
                if (faulty != "")
                    throw new Exception("Following employees have already been tagged with date range " + masterdata.FromDate + " AND " + masterdata.ToDate + ": \r\n" + faulty);




                sql = "SELECT * FROM [MST].[LayOff] where FromDate='" + masterdata.FromDate + "' and plantid='" + identity.PlantId + "' and id<>'" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (string.IsNullOrEmpty(masterdata.Id) == true)
                {
                    //if (dsMaster.Tables[0].Rows.Count > 0)
                    //throw new Exception("This date has already been assigned as working day! [" + masterdata.FromDate + "]");
                }


                sql = "SELECT * FROM [MST].[LayOff] where id='" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                sql = "SELECT * FROM [MST].[LayOffEmpList] where LayOffId='" + masterdata.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsEmployee, false, "1");


                sql = "SELECT * FROM [MST].[LayOff] where id='" + masterdata.Id + "'";
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
                    dr["Description"] = masterdata.Description;

                    dr["FromDate"] = masterdata.FromDate;
                    dr["ToDate"] = masterdata.ToDate;
                 
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);


                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    MasterID = dr["id"].ToString();
                    dr.BeginEdit();
                    dr["Description"] = masterdata.Description;

                    dr["FromDate"] = masterdata.FromDate;
                    dr["ToDate"] = masterdata.ToDate;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();

                }






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

                        IEnumerable<LayOffEmpList> filter = employeedata.Where(ee => ee.Id == dsEmployee.Tables[0].Rows[i]["EmpSystemId"].ToString());
                        if (filter == null || filter.Count() == 0)
                        {
                            dsEmployee.Tables[0].Rows[i].Delete();
                        }


                    }



                    string employeeTableID = "";
                    DateTime FromDate = Convert.ToDateTime(masterdata.FromDate);
                    DateTime ToDate = Convert.ToDateTime(masterdata.ToDate);
                    int dateIndex = 0;
                    for (int i = 0; i < employeedata.Count; i++)
                    {
                        FromDate = Convert.ToDateTime(masterdata.FromDate);

                        dsEmployee.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + employeedata[i].Id + "'";
                        if (dsEmployee.Tables[0].DefaultView.Count == 0)
                        {
                            while (FromDate <= ToDate)
                            {
                                dateIndex++;

                                if (employeeTableID == "")
                                {
                                    bplib.clsGenID objID = new bplib.clsGenID();
                                    objID.GenID("COMPENSATORY HOLIDAY EMPLOYEES", out employeeTableID);
                                }

                                DataRow dr = dsEmployee.Tables[0].NewRow();
                                dr["id"] = employeeTableID + (i + 1).ToString() + "-" + dateIndex.ToString();

                                dr["LayOffId"] = MasterID;
                                dr["EmpSystemId"] = employeedata[i].Id;
                                dr["WorkDate"] = FromDate.ToString("dd-MMM-yyyy");

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;

                                dsEmployee.Tables[0].Rows.Add(dr);


                                FromDate = FromDate.AddDays(1);
                            }
                        }

                    }
                }

                clsStaticInfo obj1 = new clsStaticInfo();
                obj1.SaveDataSets(dsMaster, dsEmployee);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

        [HttpPost]
        public JsonResult deleteemployee(LayOff masterdata, string employeedata)
        {
            try
            {
                DataSet dsMaster;
                string sql = " SELECT * FROM [MST].[LayOffEmpList] WHERE LayOffId='" + masterdata.Id + "' AND EmpSystemId='" + employeedata + "'";
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

        [HttpPost]
        public JsonResult LoadLayOff(string ID)
        {


            try
            {
                string sql = @"SELECT Id,Description, FORMAT( FromDate,'dd-MMM-yyyy') AS FromDate,FORMAT( ToDate,'dd-MMM-yyyy') AS ToDate FROM [MST].[LayOff] where ID='" + ID + "'";

                var masterData = _sqlRepository.GetModelCollection<LayOff>(sql);


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sql = @"SELECT distinct Emp.SystemID AS Id,
                                        format(EMP.DOJ,'dd-MMM-yyyy') AS DOJ,
                                        CASE WHEN doj>(CASE WHEN co.FromDate<co.ToDate THEN co.FromDate ELSE co.ToDate END) THEN 'YES' ELSE '' END AS JoinedAfter,

                                case when isnull(O.Id,'')<>'' THEN 1 ELSE 0 END as Active,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                                    EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,
                                        DEPT.UserName Department,S.UserName Section,
                                        PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant
                                        FROM EmployeeInformation EMP
                                        INNER JOIN [MST].[LayOffEmpList] O ON EMP.SystemID=o.EmpSystemID 
                                        LEFT OUTER JOIN mst.LayOff AS co ON co.Id=o.LayOffId
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        WHERE emp.PlantID='" + identity.PlantId + @"' AND O.LayOffId='" + ID + "'";


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

        public JsonResult searchEmployees(string column, string value, string offdate, string ToDate, bool IsFutureDOJAccepted)
        {
            string strKey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strKey = column + " like '%" + value + "%'";

            DateTime maxDate = Convert.ToDateTime(offdate);
            if (Convert.ToDateTime(ToDate) > maxDate)
                maxDate = Convert.ToDateTime(ToDate);

            if (Convert.ToDateTime(ToDate) < Convert.ToDateTime(offdate))
                offdate = ToDate;


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
            string sql = @"select * from (SELECT distinct Emp.SystemID AS Id,format(EMP.DOJ,'dd-MMM-yyyy') AS DOJ," + joinedAfter + @"
                                case when isnull(O.Id,'')<>'' THEN 1 ELSE 0 END as Active,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                                    EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,
                                        DEPT.UserName Department,S.UserName Section, emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,
                                        PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant
                                        FROM EmployeeInformation EMP
                                        LEFT OUTER JOIN [MST].[LayOffEmpList] O ON EMP.SystemID=o.EmpSystemID and LayOffID=(
                                            select top 1 Id from [MST].[LayOff] O where PlantID='" + identity.PlantId + @"' and FromDate='" + offdate + @"')
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


                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize, HttpPost]
        public JsonResult GetView()
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT o.Description,
                            o.Id,FORMAT( o.FromDate,'dd-MMM-yyyy') AS FromDate,
                            FORMAT( o.ToDate,'dd-MMM-yyyy') AS ToDate
                             FROM [MST].[LayOff] O
                          
                            WHERE  O.PlantId='" + identity.PlantId + @"'
                            ORDER BY O.FromDate DESC";


            try
            {


                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
    }

    public class LayOff : BaseModel
    {
        public string Id { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string Description { get; set; } = "";
        public string FromDate { get; set; } = "";
        public string ToDate { get; set; } = "";
        //public string ToDateTreatmentType { get; set; } = "";
        //public string HolidayCategoryId { get; set; } = "";
        //public bool IsFromDateOTApplicable { get; set; } = false;
        //public bool ForEntirePlant { get; set; } = false;
        ////public string AddedBy { get; set; } = "";
        //public string AddedDate { get; set; } = "";
        //public string AddedFromIP { get; set; } = "";
        //public string UpdatedBy { get; set; } = "";
        //public string UpdatedDate { get; set; } = "";
        //public string UpdatedFromIP { get; set; } = "";
    }
    public class LayOffEmpList : BaseModel
    {
        public string Id { get; set; } = "";
        public string LayOffId { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string EmpSystemId { get; set; } = "";
        //public string AddedBy { get; set; } = "";
        //public string AddedDate { get; set; } = "";
        //public string AddedFromIP { get; set; } = "";
        //public string UpdatedBy { get; set; } = "";
        //public string UpdatedDate { get; set; } = "";
        //public string UpdatedFromIP { get; set; } = "";
    }


}