using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class DailyAllowanceController : BaseController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IRestService _restService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRestDetailsService _restDetailsService;

        public DailyAllowanceController(

              ILeaveTransectionService leaveTransactionService
              , ISqlRepository sqlRepository
            , IRestService restService
             , IRestDetailsService restDetailsService
            , IUnitOfWork U
            )
        {
            _leaveTransactionService = leaveTransactionService;
            _restService = restService;
            _sqlRepository = sqlRepository;
            _unitOfWork = U;
            _restDetailsService = restDetailsService;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [Authorize]
        public ActionResult DailyAllowanceConfirmation()
        {
            return View();
        }
        [Authorize]
        public ActionResult DailyAllowanceRateEmpWise()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        //========================Setting=========================
        [HttpGet, Authorize]
        public ActionResult GetAllowanceDaily()
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT id,UserName,SalaryHeadId FROM [HKP].[AllowanceDaily]";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShiftInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 0 CheckBoxSelect, SystemID ShiftId, UserName, IsActive
                          ,format(InTime,'hh:mm tt')+'-'+ format(OutTime,'hh:mm tt')+ CASE WHEN DefaultShift=1 THEN ' (Default)' ELSE ''END  as Time
                            ,'' EffectiveTime
                            ,'' FromDate
                            ,'' ToDate
                          FROM ShiftDefination WHERE PlantID='" + identity.PlantId + @"' AND IsActive=1 
                          ORDER BY SequenceNo";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeCategoryInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 0 CheckBoxSelect, Id EmployeeCategoryId, UserName EmployeeCategory 
                         
                            ,'' Rate
                          FROM hkp.EmployeeCategory 
                          ORDER BY Sequence";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult GetDailyAllowance()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select da.Id, ad.UserName AllowanceType,sd.UserName ShiftName,format(da.EffectiveTime,'hh:mm tt')   EffectiveTime , FORMAT( da.FromDate,'dd-MMM-yyyy')  FromDate  , FORMAT( da.ToDate,'dd-MMM-yyyy')  ToDate
                           from DailyAllowanceSetting AS da
                            LEFT JOIN ShiftDefination AS sd  ON sd.SystemID = da.ShiftSystemID 
                            LEFT JOIN hkp.AllowanceDaily AS ad ON ad.Id=da.DailyAllowanceID
                            WHERE da.PlantID='" + identity.PlantId + @"' AND da.Active=1";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveDailyAllowance(string DailyAllowanceType, IEnumerable<DailyAllowance> DailyAllowanceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsDailyAllowanceSettingList;
            try
            {
                string sql = @"select * from DailyAllowanceSetting WHERE PlantID = '" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDailyAllowanceSettingList, false, "1");

                if (DailyAllowanceData.Count() > 0)
                {
                    foreach (var item in DailyAllowanceData.Where(x => x.CheckBoxSelect == true))
                    //for (int i = 0; i < DailyAllowanceData.Count(); i++)
                    {
                        if (item.CheckBoxSelect == true)
                        {
                            DataView dvDailyAllowanceSettingList = new DataView(dsDailyAllowanceSettingList.Tables[0]);
                            dvDailyAllowanceSettingList.RowFilter = "DailyAllowanceID='" + DailyAllowanceType.ToString() + "' AND ShiftSystemID='" + item.ShiftId.ToString() + "' AND PlantID='" + identity.PlantId + "'";

                            if (dvDailyAllowanceSettingList.Count == 0)
                            {
                                string sID = string.Empty;
                                bplib.clsGenID objGenID = new bplib.clsGenID();
                                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceSettingList", out sID);
                                DataRow dr = dsDailyAllowanceSettingList.Tables[0].NewRow();
                                dr["Id"] = "DA" + sID;
                                dr["PlantID"] = identity.PlantId.ToString();
                                dr["ShiftSystemID"] = item.ShiftId.ToString();
                                dr["DailyAllowanceID"] = DailyAllowanceType.ToString();
                                dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy")+" "+ item.EffectiveTime.ToString();
                                dr["FromDate"] = item.FromDate.ToString();
                                dr["ToDate"] = item.ToDate.ToString();
                                dr["Active"] = true;


                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsDailyAllowanceSettingList.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                if (Convert.ToDateTime(dvDailyAllowanceSettingList[0].Row["ToDate"].ToString()) < Convert.ToDateTime(item.FromDate.ToString()))
                                {
                                    string sID = string.Empty;
                                    bplib.clsGenID objGenID = new bplib.clsGenID();
                                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceSettingList", out sID);
                                    DataRow dr = dsDailyAllowanceSettingList.Tables[0].NewRow();
                                    dr["Id"] = "DA" + sID;
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["ShiftSystemID"] = item.ShiftId.ToString();
                                    dr["DailyAllowanceID"] = DailyAllowanceType.ToString();
                                    dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                                    dr["FromDate"] = item.FromDate.ToString();
                                    dr["ToDate"] = item.ToDate.ToString();
                                    dr["Active"] = true;

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = System.DateTime.Now.ToString();
                                    dr["AddedFromIP"] = identity.IPAddress;
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;
                                    dsDailyAllowanceSettingList.Tables[0].Rows.Add(dr);

                                }
                                else
                                {
                                    throw new Exception("From  Date must be greater than previous to date");
                                }

                            }
                            dvDailyAllowanceSettingList.RowFilter = null;
                        }

                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsDailyAllowanceSettingList);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        
        [HttpPost, Authorize]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='' AND EmpSystemId=''
                string sql = @"Delete FROM [dbo].[DailyAllowanceSetting] WHERE PlantId='" + identity.PlantId + "'  AND Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted });
        }




        [HttpGet]
        public ActionResult GetDailyAllowanceRate()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  SELECT dar.Id,dar.Rate, ad.UserName AllowanceType,ek.UserName EmployeeCategory
                          FROM DailyAllowanceRate AS dar 
                          LEFT JOIN hkp.EmployeeCategory AS EK  ON EK.id = dar.EmployeeCategoryId 
                          LEFT JOIN hkp.AllowanceDaily AS ad ON ad.Id=dar.DailyAllowanceID
                          WHERE dar.PlantID='" + identity.PlantId + @"'";
          


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SaveDailyAllowanceRate(string DailyAllowanceType, IEnumerable<DailyAllowanceRate> DailyAllowanceRateData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsDailyAllowanceRateList;
            try
            {
                string sql = @"select * from DailyAllowanceRate WHERE PlantID = '" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDailyAllowanceRateList, false, "1");

                if (DailyAllowanceRateData.Count() > 0)
                {
                    foreach (var item in DailyAllowanceRateData.Where(x => x.CheckBoxSelect == true))
                    //for (int i = 0; i < DailyAllowanceData.Count(); i++)
                    {
                        if (item.CheckBoxSelect == true)
                        {
                            DataView dvDailyAllowanceSettingList = new DataView(dsDailyAllowanceRateList.Tables[0]);
                            dvDailyAllowanceSettingList.RowFilter = "DailyAllowanceID='" + DailyAllowanceType.ToString() + "' AND EmployeeCategoryId='" + item.EmployeeCategoryId.ToString() + "'  AND PlantID='" + identity.PlantId + "'";

                            if (dvDailyAllowanceSettingList.Count == 0)
                            {
                                string sID = string.Empty;
                                bplib.clsGenID objGenID = new bplib.clsGenID();
                                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceRate", out sID);
                                DataRow dr = dsDailyAllowanceRateList.Tables[0].NewRow();
                                dr["Id"] = "DAR" + sID;
                                dr["PlantID"] = identity.PlantId.ToString();
                                dr["EmployeeCategoryId"] = item.EmployeeCategoryId.ToString();
                                dr["Rate"] = item.Rate.ToString();
                                dr["DailyAllowanceID"] = DailyAllowanceType.ToString();                              
                                dr["Active"] = true;
                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dsDailyAllowanceRateList.Tables[0].Rows.Add(dr);

                            }
                            else
                            {

                                DataRow dr = dvDailyAllowanceSettingList[0].Row;
                                dr.BeginEdit();
                                dr["PlantID"] = identity.PlantId.ToString();
                                dr["EmployeeCategoryId"] = item.EmployeeCategoryId.ToString();
                                dr["Rate"] = item.Rate.ToString();
                                dr["DailyAllowanceID"] = DailyAllowanceType.ToString();
                                dr["Active"] = true;                               
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dr.EndEdit();
                            }
                            dvDailyAllowanceSettingList.RowFilter = null;
                        }

                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsDailyAllowanceRateList);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public ActionResult DeleteRate(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='' AND EmpSystemId=''
                string sql = @"Delete FROM [dbo].[DailyAllowanceRate] WHERE PlantId='" + identity.PlantId + "'  AND Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted });
        }
        //=======================Transaction==========================
        [HttpGet]
        public ActionResult GetDailyAllowanceTransaction(string workDate,string salaryHeadId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'), 
	                                E.SystemID, E.EmployeeCode, E.EmployeeName, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ
                                    , De.UserName DepartmentName
                                    , EC.UserName EmpCategoryName     
                                    ,ISNULL(Se.UserName,'') Section 
                                    ,ISNULL(Sus.UserName,'') SubSection 
                                    ,ISNULL(U.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation, dat.Quantity 
                            FROM [DailyAllowanceTransaction] AS dat 
                            LEFT JOIN EmployeeInformation AS E  ON E.SystemId = dat.EmpSystemId
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit AS U ON U.Id= E.UnitID 
                            LEFT JOIN ORG.Division AS Dv ON Dv.Id= E.DivisionID 
                            LEFT JOIN ORG.Department AS De ON De.Id = E.DepartmentID 
                            LEFT JOIN HKP.Designation AS Dsg ON Dsg.Id= E.DesignationSystemID 
                            LEFT JOIN HKP.DesignationGroup AS DsgGr ON E.DesignationGroupID =  DsgGr.ID
                            LEFT JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                            LEFT JOIN HKP.LegalDesignation ld on ld.Id=e.LegalDesignationId
                            LEFT JOIN ORG.Section AS Se ON Se.Id= E.SectionID
                            LEFT JOIN ORG.Line eL on eL.id=e.LineId
                            LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= E.SubSectionID 
                            WHERE dat.WorkDate ='" + workDate + "' AND dat.PlantId='" + identity.PlantId + @"' AND dat.SalaryHeadId='" + salaryHeadId + "'"; 

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(IEnumerable<object> empList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            List<string> ExcEmployeeList = new List<string>();
            foreach (var item in empList)
            {
                //ExcEmployeeList.Add(item.EmpSystemId);
            }

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                string sql = @"SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

                if (ExcEmployeeList.Count > 0)
                {
                    for (int i = 0; i < ExcEmployeeList.Count; i++)
                    {
                        DataView dvExceptionEmployeeList = new DataView(dsExceptionEmployeeList.Tables[0]);
                        dvExceptionEmployeeList.RowFilter = "EmpSystemId='" + ExcEmployeeList[i].ToString() + "' AND PlantId='" + identity.PlantId + "'";
                        if (dvExceptionEmployeeList.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployee", out sID);
                            DataRow dr = dsExceptionEmployeeList.Tables[0].NewRow();
                            dr["Id"] = "EX" + sID;
                            dr["EmpSystemId"] = ExcEmployeeList[i].ToString();
                            dr["PlantId"] = identity.PlantId;
                            dr["IsActive"] = true;
                            dr["IsForever"] = true;
                            dr["WorkDate"] = System.DateTime.Now.ToString();
                            dr["ExpirationDate"] = System.DateTime.Now.ToString();
                            dr["ExceptionCategory"] = "Salary Process";
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dsExceptionEmployeeList.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvExceptionEmployeeList[0].Row;

                            dr.BeginEdit();
                            dr["PlantId"] = identity.PlantId;
                            dr["EmpSystemId"] = ExcEmployeeList[i].ToString();
                            dr["IsActive"] = true;
                            dr["IsForever"] = true;
                            dr["WorkDate"] = System.DateTime.Now.ToString();
                            dr["ExpirationDate"] = System.DateTime.Now.ToString();
                            dr["ExceptionCategory"] = "Salary Process";
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();

                        }
                        dvExceptionEmployeeList.RowFilter = null;
                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsExceptionEmployeeList);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult DeleteDetail(string id)
        {
            _restDetailsService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

    }

    public class DailyAllowance
    {
        public bool CheckBoxSelect { get; set; }
        public string ShiftId { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public bool IsSpecificTime { get; set; }
        public string Time { get; set; }
        public string EffectiveTime { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

    }
    public class DailyAllowanceRate
    {
        public bool CheckBoxSelect { get; set; }
        public string EmployeeCategoryId { get; set; }
        public string Rate { get; set; }


    }
}