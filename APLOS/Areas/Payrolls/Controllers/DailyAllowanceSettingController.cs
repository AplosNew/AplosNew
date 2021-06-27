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
    public class DailyAllowanceSettingController : BaseController
    {
        #region Constructor

        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IRestService _restService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRestDetailsService _restDetailsService;

        public DailyAllowanceSettingController(

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

        #region Constructor
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly ISqlRepository _sqlRepository;
        //public AllowanceDailyController(IUnitOfWork U, ISqlRepository R)
        //{
        //    _unitOfWork = U;
        //    _sqlRepository = R;
        //}
        #endregion



        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetAutoSequence()
        {
            string sql = @"SELECT ISNULL((MAX(Sequence)+1 ),1) Sequence FROM [HKP].[AllowanceDaily]";
            return Json(_sqlRepository.GetModelCollection<AllowanceDailyModel>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT  [Id]
                              ,[Sequence]
                              ,[Code]
                              ,[ShortName]
                              ,[StandardName]
                              ,[UserName]
                              ,[Description]
                              ,[Remarks]
                              ,[Active]
                              ,[AddedBy]
                              ,[AddedDate]
                              ,[AddedFromIP]
                              ,[UpdatedBy]
                              ,[UpdatedDate]
                              ,[UpdatedFromIP]
                              ,[SalaryHeadId]
                              ,[IsAllDesignation]
                              ,[IsFixed]
                              ,[Rate]
                              ,[IsAllShift]
                              ,[EffectiveTime]
                              ,[IsSpecificTime]
                              ,[PlantId]
                              ,[FormulaDescription]
                              ,[FormulaDesID]
                              ,[CalculationBasics]
                              ,[Catagory],IsRateBasedOnSalaryRange,SalaryRangeBasedOnSalaryHeadId,IsVoucherPayment
                              ,FORMAT ([FromEffectiveDate],'dd-MMM-yyyy') [FromEffectiveDate]
                              ,FORMAT ([ToEffectiveDate],'dd-MMM-yyyy') [ToEffectiveDate]
                              FROM [HKP].[AllowanceDaily] 
                              WHERE PlantID='" + identity.PlantId + @"' ORDER BY [Sequence] ";
            return Json(_sqlRepository.GetModelCollection<AllowanceDailyModel>(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var sql = @"SELECT Id, UserName FROM [HKP].[AllowanceDaily] ORDER BY UserName";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AllowanceDailyModel entity)
        {
            try
            {
                SaveData(entity);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            string idFromDB = string.Empty;
            string Id = string.Empty;

            bplib.clsGenID objGenID = null;
            objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AllowanceDaily", out idFromDB);
            Id = "AD" + idFromDB;
            sID = Id.Trim();
            return sID;

        }

        private DataSet CheckCode(string Code, string id)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT Code FROM [HKP].[AllowanceDaily] WHERE Code='" + Code + "' and Id <>'" + id + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private DataSet CheckUserName(string UserName, string id,string PlantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT UserName FROM [HKP].[AllowanceDaily] WHERE UserName='" + UserName + "' and Id <>'" + id + "' and PlantId ='" + PlantId + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private void SaveData(AllowanceDailyModel data)
        {
            var code = CheckCode(data.Code, data.Id);
            if (code.Tables[0].Rows.Count > 0)
            {
                throw new Exception("Code already exist.");
            }

            var UserName = CheckUserName(data.UserName, data.Id, data.PlantId);
            if (UserName.Tables[0].Rows.Count > 0)
            {
                throw new Exception("UserName already exist.");
            }

            
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "SELECT * FROM [HKP].[AllowanceDaily] WHERE ID='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                    dr["Code"] = data.Code;
                    dr["ShortName"] = data.ShortName;
                    dr["StandardName"] = data.StandardName;
                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Sequence"] = data.Sequence;
                    dr["FromEffectiveDate"] = data.FromEffectiveDate;
                    dr["ToEffectiveDate"] = data.ToEffectiveDate;
                    dr["Active"] = data.Active;
                    dr["SalaryHeadId"] = data.SalaryHeadId;
                    dr["FormulaDescription"] = data.FormulaDescription;
                    dr["FormulaDesID"] = data.FormulaDesID;
                    dr["CalculationBasics"] = data.CalculationBasics;
                    dr["IsRateBasedOnSalaryRange"] = data.IsRateBasedOnSalaryRange;
                    dr["SalaryRangeBasedOnSalaryHeadId"] = data.SalaryRangeBasedOnSalaryHeadId;
                    dr["IsVoucherPayment"] = data.IsVoucherPayment;
                    dr["Catagory"] = data.Catagory;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["PlantId"] = identity.PlantId;
                    dr["IsAllDesignation"] = true;
                    dr["IsFixed"] = true;
                    dr["Rate"] = 0;
                    dr["IsAllShift"] = true;
                    dr["IsSpecificTime"] = false;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["Code"] = data.Code;
                    dr["Sequence"] = data.Sequence;
                    dr["ShortName"] = data.ShortName;
                    dr["StandardName"] = data.StandardName;
                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Active"] = data.Active;
                    dr["SalaryHeadId"] = data.SalaryHeadId;
                    dr["FormulaDescription"] = data.FormulaDescription;
                    dr["FormulaDesID"] = data.FormulaDesID;
                    dr["CalculationBasics"] = data.CalculationBasics;
                    dr["FromEffectiveDate"] = data.FromEffectiveDate;
                    dr["ToEffectiveDate"] = data.ToEffectiveDate;
                    dr["Catagory"] = data.Catagory;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["PlantId"] = identity.PlantId;
                    dr["IsRateBasedOnSalaryRange"] = data.IsRateBasedOnSalaryRange;
                    if (data.IsRateBasedOnSalaryRange)
                    {
                        dr["IsAllDesignation"] = true;
                    }
                    dr["SalaryRangeBasedOnSalaryHeadId"] = data.SalaryRangeBasedOnSalaryHeadId;
                    dr["IsVoucherPayment"] = data.IsVoucherPayment;
                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }

        [HttpPost,Authorize]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [HKP].[AllowanceDaily] WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        [HttpGet, Authorize]
        public JsonResult GetSalaryHeadCbo()
        {
            var sql = @"SELECT SalaryHeadID, SalaryHead FROM SalaryHead ORDER BY SalaryHead";
            return Json(_sqlRepository.GetCombo(sql, "SalaryHeadID", "SalaryHead"), JsonRequestBehavior.AllowGet);
        }
        #endregion


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


        //[HttpGet, Authorize]
        //public ActionResult xGetShiftInfo()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string sql = @"SELECT 0 CheckBoxSelect, SystemID ShiftId, UserName, IsActive
        //                  ,format(InTime,'hh:mm tt')+'-'+ format(OutTime,'hh:mm tt')+ CASE WHEN DefaultShift=1 THEN ' (Default)' ELSE ''END  as Time
        //                    ,'' EffectiveTime
        //                    ,'' FromDate
        //                    ,'' ToDate
        //                  FROM ShiftDefination WHERE PlantID='" + identity.PlantId + @"' AND IsActive=1 
        //                  ORDER BY SequenceNo";

        //    var data = _sqlRepository.GetDataCollection(sql);

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
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


        [HttpGet,Authorize]
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
        #region Master Update on add shift and designation
        [HttpPost, Authorize]
        public ActionResult UpdateMasterForDesignation(AllowanceDailyModel data)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsDetails;
            try
            {
                string sql1 = @"Delete FROM [dbo].[DailyAllowanceRate] WHERE PlantId='" + identity.PlantId + "'  AND DailyAllowanceID='" + data.Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsDetails, false, "1");


                string sql = "SELECT * FROM [HKP].[AllowanceDaily] WHERE ID='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();


                    dr["IsAllDesignation"] = data.IsAllDesignation;
                    //dr["IsFixed"] = data.IsFixed;
                    //dr["Rate"] = data.Rate;
                    dr["IsFixed"] = data.IsFixed;
                    if (data.IsFixed)
                    {
                        if (!string.IsNullOrEmpty(data.Rate))
                        {
                            dr["Rate"] = data.Rate;
                        }

                        dr["FormulaDescription"] = null;

                        dr["FormulaDesID"] = null;

                    }
                    else
                    {

                        dr["Rate"] = 0;

                        if (!string.IsNullOrEmpty(data.FormulaDescription))
                        {
                            dr["FormulaDescription"] = data.FormulaDescription.ToString();
                        }
                        if (!string.IsNullOrEmpty(data.FormulaDesID))
                        {
                            dr["FormulaDesID"] = data.FormulaDesID.ToString();
                        }
                    }
                    //dr["FormulaDescription"] = data.FormulaDescription;
                    //dr["FormulaDesID"] = data.FormulaDesID;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public ActionResult UpdateMasterForShift(AllowanceDailyModel data)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsDetails;
            try
            {


                string sql1 = @"Delete FROM [dbo].[DailyAllowanceSetting] WHERE PlantId='" + identity.PlantId + "'  AND DailyAllowanceID='" + data.Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsDetails, false, "1");


                string sql = "SELECT * FROM [HKP].[AllowanceDaily] WHERE ID='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();


                    dr["IsAllShift"] = data.IsAllShift;
                    dr["IsSpecificTime"] = data.IsSpecificTime;
                    if (data.IsSpecificTime)
                    {
                        //dr["EffectiveTime"] = data.EffectiveTime;
                        dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(data.EffectiveTime).ToString("hh:mm tt");
                    }
                    else
                    {
                        dr["EffectiveTime"] = DBNull.Value;
                    }



                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw (ex);
            }
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion



        #region Shift
        [HttpGet, Authorize]
        public ActionResult GetShiftInfo(string DailyAllowanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CheckBoxSelect= CONVERT(BIT, 0) ---CheckBoxSelect=CASE WHEN das.DailyAllowanceId IS NOT NULL THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END
                            ,sd.SystemID ShiftId
                            ,sd.UserName
                            ,sd.IsActive
                            ,format(sd.InTime,'hh:mm tt')+'-'+ format(sd.OutTime,'hh:mm tt')+ CASE WHEN sd.DefaultShift=1 THEN ' (Default)' ELSE ''END  as Time
                            ,das.EffectiveTime
                            ,das.IsSpecificTime
                            ,das.Id
                           , Status=CASE WHEN das.DailyAllowanceId IS NOT NULL THEN 'Active' ELSE '' END          
                            FROM ShiftDefination sd 
                            LEFT JOIN DailyAllowanceSetting AS das  ON sd.SystemID = das.ShiftSystemID  AND das.DailyAllowanceId='" + DailyAllowanceId + @"' AND das.PlantID='" + identity.PlantId + @"'   
                            WHERE sd.PlantID='" + identity.PlantId + @"' AND sd.IsActive=1 
                            ORDER BY sd.SequenceNo ";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public JsonResult SaveDailyAllowance(string DailyAllowanceType, IEnumerable<DailyAllowance> DailyAllowanceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsDailyAllowanceSettingList;
            try
            {

                DataSet dsMaster;
                string sqls = "SELECT * FROM [HKP].[AllowanceDaily] WHERE ID='" + DailyAllowanceType + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqls, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["IsAllShift"] = false;
                    dr["IsSpecificTime"] = false;
                    dr["EffectiveTime"] = DBNull.Value;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);


                if (DailyAllowanceData.Count() > 0)
                {
                    foreach (var item in DailyAllowanceData.Where(x => x.CheckBoxSelect == true))
                    //for (int i = 0; i < DailyAllowanceData.Count(); i++)
                    {
                        if (item.CheckBoxSelect == true)
                        {
                            string sql = @"select * from DailyAllowanceSetting WHERE DailyAllowanceID='" + DailyAllowanceType.ToString() + "' AND ShiftSystemID='" + item.ShiftId.ToString() + "' AND PlantID='" + identity.PlantId + "'";
                            objCon = new ConnectionManager.DAL.ConManager("1");
                            objCon.OpenDataSetThroughAdapter(sql, out dsDailyAllowanceSettingList, false, "1");
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
                                //dr["IsSpecificTime"] = item.IsSpecificTime;
                                dr["DailyAllowanceID"] = DailyAllowanceType.ToString();
                                //dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + item.EffectiveTime.ToString();
                                //dr["FromDate"] = item.FromDate.ToString();
                                //dr["ToDate"] = item.ToDate.ToString();

                                dr["IsSpecificTime"] = item.IsSpecificTime;
                                if (item.IsSpecificTime)
                                {
                                    //dr["EffectiveTime"] = data.EffectiveTime;
                                    dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(item.EffectiveTime).ToString("hh:mm tt");
                                }
                                else
                                {
                                    dr["EffectiveTime"] = DBNull.Value;
                                }
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

                                DataRow dr = dsDailyAllowanceSettingList.Tables[0].DefaultView[0].Row;
                                //DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["PlantID"] = identity.PlantId.ToString();
                                dr["ShiftSystemID"] = item.ShiftId.ToString();
                                dr["DailyAllowanceID"] = DailyAllowanceType.ToString();
                                //dr["IsSpecificTime"] = item.IsSpecificTime;
                                //dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy") + " " +Convert.ToDateTime( item.EffectiveTime).ToString("hh:mm tt");
                                dr["IsSpecificTime"] = item.IsSpecificTime;
                                if (item.IsSpecificTime)
                                {
                                    //dr["EffectiveTime"] = data.EffectiveTime;
                                    dr["EffectiveTime"] = System.DateTime.Now.ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(item.EffectiveTime).ToString("hh:mm tt");
                                }
                                else
                                {
                                    dr["EffectiveTime"] = DBNull.Value;
                                }
                                //dr["FromDate"] = item.FromDate.ToString();
                                //dr["ToDate"] = item.ToDate.ToString();
                                dr["Active"] = true;

                                //dr["AddedBy"] = identity.Name;
                                //dr["AddedDate"] = System.DateTime.Now.ToString();
                                //dr["AddedFromIP"] = identity.IPAddress;
                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dr.EndEdit();



                            }
                            dvDailyAllowanceSettingList.RowFilter = null;
                            obj.SaveDataSets(dsDailyAllowanceSettingList);
                        }

                    }
                }


            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }


        [HttpPost, Authorize]
        public ActionResult DeleteShift(string Id)
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
        #endregion



        #region Designation wise rate

        [HttpGet,Authorize]
        public ActionResult GetDailyAllowanceRate(string DailyAllowanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CheckBoxSelect= CONVERT(BIT, 0) 
---CheckBoxSelect=CASE WHEN dar.DailyAllowanceId IS NOT NULL THEN CONVERT(BIT, 1) ELSE CONVERT(BIT, 0) END
                            ,d.Id DesignationId                           
                            ,d.UserName DesignationName
                            ,dm.UserName DesignationGroup
                            ,dar.Rate
                            ,dar.Rate RateOld
                            ,dar.Id
                            ,dar.FormulaDescription
                            ,dar.FormulaDesID
                            ,dar.IsFixed
                            --,ad.UserName DailyAllowanceName 
                            ,IsRate=CASE WHEN dar.Rate IS NOT NULL THEN 1 ELSE 0 END
                            , Status=CASE WHEN dar.DailyAllowanceId IS NOT NULL THEN 'Active' ELSE '' END , EC.UserName  Category 
                            , STUFF((SELECT '; ' + LD.UserName 
                                      FROM [HKP].[LegalDesignation] LD
                                      LEFT JOIN [MST].[DesignationMasterLegalDesignation] DML ON DML.LegalDesignationId = LD.Id 
                                      WHERE DML.DesignationMasterId = dm.Id 
                                      ORDER BY UserName
                                      FOR XML PATH('')), 1, 1, '') [LegalDesignation]
                            FROM hkp.Designation AS d
                            LEFT JOIN [MST].[DesignationMaster] AS dm ON dm.DesignationId = d.Id
                            LEFT JOIN hkp.DesignationGroup AS dg ON dg.Id = dm.DesignationGroupId
                            LEFT JOIN DailyAllowanceRate AS dar  ON d.id = dar.DesignationId  AND dar.DailyAllowanceId='" + DailyAllowanceId + @"' AND dar.PlantID='" + identity.PlantId + @"'   
                            LEFT JOIN hkp.EmployeeCategory AS EC ON EC.Id=dm.EmployeeCategoryId  
                            --LEFT JOIN hkp.AllowanceDaily AS ad ON ad.Id=dar.DailyAllowanceId
                           ";



            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost,Authorize]
        public JsonResult SaveDailyAllowanceRate(string DailyAllowanceType, IEnumerable<DailyAllowanceRateModel> DailyAllowanceRateData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsDailyAllowanceRateList;
            try
            {


                DataSet dsMaster;
                string sqls = "SELECT * FROM [HKP].[AllowanceDaily] WHERE ID='" + DailyAllowanceType + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqls, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["IsAllDesignation"] = false;
                    dr["IsFixed"] = false;
                    dr["Rate"] = 0;
                    dr["FormulaDescription"] = null;
                    dr["FormulaDesID"] = null;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
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
                            dvDailyAllowanceSettingList.RowFilter = "DailyAllowanceID='" + DailyAllowanceType.ToString() + "' AND DesignationId='" + item.DesignationId.ToString() + "'  AND PlantID='" + identity.PlantId + "'";

                            if (dvDailyAllowanceSettingList.Count == 0)
                            {
                                string sID = string.Empty;
                                bplib.clsGenID objGenID = new bplib.clsGenID();
                                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceRate", out sID);
                                DataRow dr = dsDailyAllowanceRateList.Tables[0].NewRow();
                                dr["Id"] = "DAR" + sID;
                                dr["PlantID"] = identity.PlantId.ToString();
                                //dr["EmployeeCategoryId"] = item.EmployeeCategoryId.ToString();
                                dr["DesignationId"] = item.DesignationId.ToString();
                                dr["IsFixed"] = item.IsFixed;
                                if (item.IsFixed)
                                {
                                    if (!string.IsNullOrEmpty(item.Rate))
                                    {
                                        dr["Rate"] = item.Rate.ToString();
                                    }

                                    dr["FormulaDescription"] = null;

                                    dr["FormulaDesID"] = null;

                                }
                                else
                                {

                                    dr["Rate"] = 0;

                                    if (!string.IsNullOrEmpty(item.FormulaDescription))
                                    {
                                        dr["FormulaDescription"] = item.FormulaDescription.ToString();
                                    }
                                    if (!string.IsNullOrEmpty(item.FormulaDesID))
                                    {
                                        dr["FormulaDesID"] = item.FormulaDesID.ToString();
                                    }
                                }
                                //if (!string.IsNullOrEmpty(item.Rate))
                                //{
                                //    dr["Rate"] = item.Rate.ToString();
                                //}
                                //if (!string.IsNullOrEmpty(item.FormulaDescription))
                                //{
                                //    dr["FormulaDescription"] = item.FormulaDescription.ToString();
                                //}
                                //if (!string.IsNullOrEmpty(item.FormulaDesID))
                                //{
                                //    dr["FormulaDesID"] = item.FormulaDesID.ToString();
                                //}


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
                                //dr["EmployeeCategoryId"] = item.EmployeeCategoryId.ToString();
                                dr["DesignationId"] = item.DesignationId.ToString();
                                dr["IsFixed"] = item.IsFixed;
                                //if (!string.IsNullOrEmpty(item.Rate))
                                //{
                                //    dr["Rate"] = item.Rate.ToString();
                                //}
                                //if (!string.IsNullOrEmpty(item.FormulaDescription))
                                //{
                                //    dr["FormulaDescription"] = item.FormulaDescription.ToString();
                                //}
                                //if (!string.IsNullOrEmpty(item.FormulaDesID))
                                //{
                                //    dr["FormulaDesID"] = item.FormulaDesID.ToString();
                                //}
                                if (item.IsFixed)
                                {
                                    if (!string.IsNullOrEmpty(item.Rate))
                                    {
                                        dr["Rate"] = item.Rate.ToString();
                                    }

                                    dr["FormulaDescription"] = null;

                                    dr["FormulaDesID"] = null;

                                }
                                else
                                {

                                    dr["Rate"] = 0;

                                    if (!string.IsNullOrEmpty(item.FormulaDescription))
                                    {
                                        dr["FormulaDescription"] = item.FormulaDescription.ToString();
                                    }
                                    if (!string.IsNullOrEmpty(item.FormulaDesID))
                                    {
                                        dr["FormulaDesID"] = item.FormulaDesID.ToString();
                                    }
                                }
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
                obj.SaveDataSets(dsDailyAllowanceRateList, dsMaster);
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
        #endregion


        #region Salary Range wise rate

        [HttpGet,Authorize]
        public ActionResult GetDailyAllowanceRateBasedOnSalaryRange(string DailyAllowanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CheckBoxSelect= CONVERT(BIT, 0)                            
                            ,R.Rate
                            ,R.SalaryRangeUpperLimit
                            ,R.Id
                            ,R.SalaryRangeLowerLimit
                            
                           
                                 
                            FROM [DailyAllowanceRateBasedOnSalaryRange] AS R
                            ---LEFT JOIN SalaryHead AS SH  ON R.SalaryHeadId = SH.SalaryHeadID 
                          
                            WHERE  R.DailyAllowanceId='" + DailyAllowanceId + @"' AND R.PlantID='" + identity.PlantId + @"'   ";



            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost,Authorize]
        public JsonResult SaveDailyAllowanceRateBasedOnSalaryRange(string DailyAllowanceType, DailyAllowanceRateBasedOnSalaryRangeModel DailyAllowanceRateData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsDailyAllowanceRateList;
            DataSet dsDailyAllowanceRateList1;
            DataSet dsDailyAllowanceRateList2;
            try
            {
                string sql1 = @"select * from [DailyAllowanceRateBasedOnSalaryRange] WHERE DailyAllowanceID='" + DailyAllowanceType.ToString() + "'  AND PlantID='" + identity.PlantId + "' AND " + DailyAllowanceRateData.SalaryRangeLowerLimit + @" BETWEEN  SalaryRangeLowerLimit and SalaryRangeUpperLimit";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsDailyAllowanceRateList1, false, "1");

                string sql2 = @"select * from [DailyAllowanceRateBasedOnSalaryRange] WHERE DailyAllowanceID='" + DailyAllowanceType.ToString() + "'  AND PlantID='" + identity.PlantId + "' AND " + DailyAllowanceRateData.SalaryRangeUpperLimit + @" BETWEEN  SalaryRangeLowerLimit and SalaryRangeUpperLimit";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsDailyAllowanceRateList2, false, "1");

               

                if (dsDailyAllowanceRateList1.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Please enter valied data.");
                }
              
                if (dsDailyAllowanceRateList2.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Please enter valied data.");
                }









                string sql = @"select * from [DailyAllowanceRateBasedOnSalaryRange] WHERE PlantID = '" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDailyAllowanceRateList, false, "1");

               



                DataView dvDailyAllowanceSettingList = new DataView(dsDailyAllowanceRateList.Tables[0]);
                dvDailyAllowanceSettingList.RowFilter = "DailyAllowanceID='" + DailyAllowanceType.ToString() + "'  AND PlantID='" + identity.PlantId + "' AND Id=''";

                if (dvDailyAllowanceSettingList.Count == 0)
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceRateSR", out sID);
                    DataRow dr = dsDailyAllowanceRateList.Tables[0].NewRow();
                    dr["Id"] = "DARSR" + sID;
                    dr["PlantID"] = identity.PlantId.ToString();
                    dr["DailyAllowanceID"] = DailyAllowanceType.ToString();
                    dr["SalaryRangeUpperLimit"] = DailyAllowanceRateData.SalaryRangeUpperLimit;
                    dr["SalaryRangeLowerLimit"] = DailyAllowanceRateData.SalaryRangeLowerLimit;
                    dr["Rate"] = DailyAllowanceRateData.Rate;
                    //dr["SalaryHeadId"] = DailyAllowanceRateData.SalaryHeadId;

                    dr["Active"] = true;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsDailyAllowanceRateList.Tables[0].Rows.Add(dr);

                }

                dvDailyAllowanceSettingList.RowFilter = null;


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
        public ActionResult DeleteRateBasedOnSalaryRange(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='' AND EmpSystemId=''
                string sql = @"Delete FROM [dbo].[DailyAllowanceRateBasedOnSalaryRange] WHERE PlantId='" + identity.PlantId + "'  AND Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion





        //=======================Transaction==========================
        [HttpGet, Authorize]
        public ActionResult GetDailyAllowanceTransaction(string workDate, string salaryHeadId)
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

        //[HttpPost, Authorize]
        //public JsonResult xCreate(IEnumerable<object> empList)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    List<string> ExcEmployeeList = new List<string>();
        //    foreach (var item in empList)
        //    {
        //        //ExcEmployeeList.Add(item.EmpSystemId);
        //    }

        //    ConnectionManager.DAL.ConManager objCon;
        //    DataSet dsExceptionEmployeeList;

        //    try
        //    {
        //        string sql = @"SELECT * FROM [dbo].[ExceptionEmployee] WHERE PlantId='" + identity.PlantId + "'";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

        //        if (ExcEmployeeList.Count > 0)
        //        {
        //            for (int i = 0; i < ExcEmployeeList.Count; i++)
        //            {
        //                DataView dvExceptionEmployeeList = new DataView(dsExceptionEmployeeList.Tables[0]);
        //                dvExceptionEmployeeList.RowFilter = "EmpSystemId='" + ExcEmployeeList[i].ToString() + "' AND PlantId='" + identity.PlantId + "'";
        //                if (dvExceptionEmployeeList.Count == 0)
        //                {
        //                    string sID = string.Empty;
        //                    bplib.clsGenID objGenID = new bplib.clsGenID();
        //                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExceptionEmployee", out sID);
        //                    DataRow dr = dsExceptionEmployeeList.Tables[0].NewRow();
        //                    dr["Id"] = "EX" + sID;
        //                    dr["EmpSystemId"] = ExcEmployeeList[i].ToString();
        //                    dr["PlantId"] = identity.PlantId;
        //                    dr["IsActive"] = true;
        //                    dr["IsForever"] = true;
        //                    dr["WorkDate"] = System.DateTime.Now.ToString();
        //                    dr["ExpirationDate"] = System.DateTime.Now.ToString();
        //                    dr["ExceptionCategory"] = "Salary Process";
        //                    dr["AddedBy"] = identity.Name;
        //                    dr["AddedDate"] = System.DateTime.Now.ToString();
        //                    dr["AddedFromIP"] = identity.IPAddress;
        //                    dr["UpdatedBy"] = identity.Name;
        //                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //                    dr["UpdatedFromIP"] = identity.IPAddress;
        //                    dsExceptionEmployeeList.Tables[0].Rows.Add(dr);


        //                }
        //                else
        //                {
        //                    //edit
        //                    DataRow dr = dvExceptionEmployeeList[0].Row;

        //                    dr.BeginEdit();
        //                    dr["PlantId"] = identity.PlantId;
        //                    dr["EmpSystemId"] = ExcEmployeeList[i].ToString();
        //                    dr["IsActive"] = true;
        //                    dr["IsForever"] = true;
        //                    dr["WorkDate"] = System.DateTime.Now.ToString();
        //                    dr["ExpirationDate"] = System.DateTime.Now.ToString();
        //                    dr["ExceptionCategory"] = "Salary Process";
        //                    dr["UpdatedBy"] = identity.Name;
        //                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
        //                    dr["UpdatedFromIP"] = identity.IPAddress;
        //                    dr.EndEdit();

        //                }
        //                dvExceptionEmployeeList.RowFilter = null;
        //            }
        //        }
        //        clsStaticInfo obj = new clsStaticInfo();
        //        obj.SaveDataSets(dsExceptionEmployeeList);

        //    }
        //    catch (Exception ex)
        //    {

        //        throw (ex);
        //    }

        //    return Json(new { Message = AplosMessage.Success });
        //}
        [HttpPost, Authorize]
        public ActionResult DeleteDetail(string id)
        {
            _restDetailsService.DeleteDetail(id);
            return Json(new { Message = AplosMessage.Deleted });
        }




        #region Additional Policy

        [HttpGet, Authorize]
        public ActionResult GetAdditionalPolicyList(string DailyAllowanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT D.ID ,D.IsEarlyOutApplicable,D.IsLunchOutApplicable,D.IsLateInApplicable,D.IsLunchOutApplicable
                                ,D.IsAbsentApplicable,D.IsLateApplicable,D.IsRouteApplicableForLate
                            ,D.IsLeaveApplicable,D.IsLeaveWithOutPayApplicable,D.EOLIFromValue,D.EOLIToValue,D.LunchOutFromValue,D.LunchOutToValue,D.AbsentFromValue
                            ,D.AbsentToValue,D.LateFromValue,D.LateToValue,D.LeaveFromValue,D.LeaveToValue,D.LeaveWithOutPayFromValue,D.LeaveWithOutPayToValue 
                            ---,D.FixedOrFormula
                            ,D.EOLIFromValue	                        
                            FROM [dbo].[DailyAllowanceAdditionalPolicy] D                                                    
                            WHERE D.DailyAllowanceId ='" + DailyAllowanceId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveDetailsChildList(string DetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select Active=case when  D.Id is null then  CONVERT(bit,0) else  CONVERT(bit,1) end ,LT.Id,LT.LeaveTypeId as LeaveId,LT.IsPreApplied,LET.UserName
                                FROM [dbo].[DailyAllowanceAdditionalPolicy] D
                                LEFT JOIN [dbo].[DailyAllowanceLeaveType] LT on  LT.DailyAllowanceAdditionalPolicyId=D.ID
							LEFT JOIN LeaveType LET ON LET.Id=LT.LeaveTypeId
							where lt.DailyAllowanceAdditionalPolicyId='" + DetailsId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveList(string AttdnBonusPmtPolicyDetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select distinct UserName,lt.Id as LeaveId ,  IsPreApplied = ABLT.IsPreApplied 
                            ,CheckBoxSelect=case when  ABLT.DailyAllowanceAdditionalPolicyId is null then  CONVERT(bit,0) else  CONVERT(bit,1) end 
                             ,ABLT.DailyAllowanceAdditionalPolicyId--,ABLT.AttdnBonusPmtPolicyMasterId
									from LeaveType lt
									LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
									  LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + identity.PlantId + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
										LEFT JOIN [dbo].[DailyAllowanceLeaveType] ABLT ON ABLT.LeaveTypeId=LT.Id and ABLT.DailyAllowanceAdditionalPolicyId='" + AttdnBonusPmtPolicyDetailsId + @"'
									 where CompanyGroupId='" + identity.CompanyGroupId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveDailyAllowanceAdditionalPolicy(DailyAllowanceAdditionalPolicy DailyAllowanceAdditionalPolicyData,List<DailyAllowanceLeaveType> LeaveList,string DailyAllowanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsDailyAllowanceAdditionalPolicy;
            try
            {
                string DailyAllowanceAdditionalPolicyId = string.Empty;
                string sql = "SELECT * FROM [dbo].[DailyAllowanceAdditionalPolicy] WHERE DailyAllowanceId='" + DailyAllowanceId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDailyAllowanceAdditionalPolicy, false, "1");


                if (dsDailyAllowanceAdditionalPolicy.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsDailyAllowanceAdditionalPolicy.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceAdditionalPolicy", out sID);
                    DailyAllowanceAdditionalPolicyId = "APC" + sID;
                    dr["ID"] = DailyAllowanceAdditionalPolicyId;
                    dr["DailyAllowanceId"] = DailyAllowanceId;

                    dr["IsEarlyOutApplicable"] = DailyAllowanceAdditionalPolicyData.IsEarlyOutApplicable;
                    dr["IsLateInApplicable"] = DailyAllowanceAdditionalPolicyData.IsLateInApplicable;
                    dr["IsLunchOutApplicable"] = DailyAllowanceAdditionalPolicyData.IsLunchOutApplicable;
                    dr["IsAbsentApplicable"] = DailyAllowanceAdditionalPolicyData.IsAbsentApplicable;
                    dr["IsLateApplicable"] = DailyAllowanceAdditionalPolicyData.IsLateApplicable;
                    dr["IsRouteApplicableForLate"] = DailyAllowanceAdditionalPolicyData.IsRouteApplicableForLate;
                    dr["IsLeaveApplicable"] = DailyAllowanceAdditionalPolicyData.IsLeaveApplicable;
                    dr["IsLeaveWithOutPayApplicable"] = DailyAllowanceAdditionalPolicyData.IsLeaveWithOutPayApplicable;
                    //dr["FixedValue"] = DailyAllowanceAdditionalPolicyData.FixedValue;
                    dr["EOLIFromValue"] = DailyAllowanceAdditionalPolicyData.EOLIFromValue;
                    dr["EOLIToValue"] = DailyAllowanceAdditionalPolicyData.EOLIToValue;
                    dr["LunchOutFromValue"] = DailyAllowanceAdditionalPolicyData.LunchOutFromValue;
                    dr["LunchOutToValue"] = DailyAllowanceAdditionalPolicyData.LunchOutToValue;
                    dr["AbsentFromValue"] = DailyAllowanceAdditionalPolicyData.AbsentFromValue;
                    dr["AbsentToValue"] = DailyAllowanceAdditionalPolicyData.AbsentToValue;
                    dr["LateFromValue"] = DailyAllowanceAdditionalPolicyData.LateFromValue;
                    dr["LateToValue"] = DailyAllowanceAdditionalPolicyData.LateToValue;
                    dr["LeaveFromValue"] = DailyAllowanceAdditionalPolicyData.LeaveFromValue;
                    dr["LeaveToValue"] = DailyAllowanceAdditionalPolicyData.LeaveToValue;
                    dr["LeaveWithOutPayFromValue"] = DailyAllowanceAdditionalPolicyData.LeaveWithOutPayFromValue;
                    dr["LeaveWithOutPayToValue"] = DailyAllowanceAdditionalPolicyData.LeaveWithOutPayToValue;

                    dsDailyAllowanceAdditionalPolicy.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsDailyAllowanceAdditionalPolicy.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    DailyAllowanceAdditionalPolicyId = dr["ID"].ToString();

                    dr["DailyAllowanceId"] = DailyAllowanceId;

                    dr["IsEarlyOutApplicable"] = DailyAllowanceAdditionalPolicyData.IsEarlyOutApplicable;
                    dr["IsLateInApplicable"] = DailyAllowanceAdditionalPolicyData.IsLateInApplicable;
                    dr["IsLunchOutApplicable"] = DailyAllowanceAdditionalPolicyData.IsLunchOutApplicable;
                    dr["IsAbsentApplicable"] = DailyAllowanceAdditionalPolicyData.IsAbsentApplicable;
                    dr["IsLateApplicable"] = DailyAllowanceAdditionalPolicyData.IsLateApplicable;
                    dr["IsRouteApplicableForLate"] = DailyAllowanceAdditionalPolicyData.IsRouteApplicableForLate;
                    dr["IsLeaveApplicable"] = DailyAllowanceAdditionalPolicyData.IsLeaveApplicable;
                    dr["IsLeaveWithOutPayApplicable"] = DailyAllowanceAdditionalPolicyData.IsLeaveWithOutPayApplicable;
                    //dr["FixedValue"] = DailyAllowanceAdditionalPolicyData.FixedValue;
                    dr["EOLIFromValue"] = DailyAllowanceAdditionalPolicyData.EOLIFromValue;
                    dr["EOLIToValue"] = DailyAllowanceAdditionalPolicyData.EOLIToValue;
                    dr["LunchOutFromValue"] = DailyAllowanceAdditionalPolicyData.LunchOutFromValue;
                    dr["LunchOutToValue"] = DailyAllowanceAdditionalPolicyData.LunchOutToValue;
                    dr["AbsentFromValue"] = DailyAllowanceAdditionalPolicyData.AbsentFromValue;
                    dr["AbsentToValue"] = DailyAllowanceAdditionalPolicyData.AbsentToValue;
                    dr["LateFromValue"] = DailyAllowanceAdditionalPolicyData.LateFromValue;
                    dr["LateToValue"] = DailyAllowanceAdditionalPolicyData.LateToValue;
                    dr["LeaveFromValue"] = DailyAllowanceAdditionalPolicyData.LeaveFromValue;
                    dr["LeaveToValue"] = DailyAllowanceAdditionalPolicyData.LeaveToValue;
                    dr["LeaveWithOutPayFromValue"] = DailyAllowanceAdditionalPolicyData.LeaveWithOutPayFromValue;
                    dr["LeaveWithOutPayToValue"] = DailyAllowanceAdditionalPolicyData.LeaveWithOutPayToValue;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsDailyAllowanceAdditionalPolicy);
               


                DataSet dsMaster;
                DataSet dsLeaveType;
                try
                {
                    string sql1 = "DELETE FROM [dbo].[DailyAllowanceLeaveType] WHERE DailyAllowanceAdditionalPolicyId='" + DailyAllowanceAdditionalPolicyId + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql1, out dsLeaveType, false, "1");

                    if (LeaveList !=null)
                    {
                        foreach (var item in LeaveList)
                        {
                            string sqll = "SELECT * FROM [dbo].[DailyAllowanceLeaveType] WHERE ID='" + item.Id + "' ";
                            objCon = new ConnectionManager.DAL.ConManager("1");
                            objCon.OpenDataSetThroughAdapter(sqll, out dsMaster, false, "1");

                            if (dsMaster.Tables[0].Rows.Count == 0)
                            {
                                DataRow dr = dsMaster.Tables[0].NewRow();
                                string sID = string.Empty;
                                bplib.clsGenID objGenID = new bplib.clsGenID();
                                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceLeaveType", out sID);
                                dr["ID"] = "ALT" + sID;

                                dr["DailyAllowanceAdditionalPolicyId"] = DailyAllowanceAdditionalPolicyId;
                                dr["LeaveTypeId"] = item.LeaveId;
                                dr["IsPreApplied"] = item.IsPreApplied;
                                dsMaster.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                //dr["AttdnBonusPmtPolicyMasterId"] = MasterId;
                                dr["DailyAllowanceAdditionalPolicyId"] = DailyAllowanceAdditionalPolicyId;
                                dr["LeaveTypeId"] = item.LeaveId;
                                dr["IsPreApplied"] = item.IsPreApplied;
                                dr.EndEdit();
                            }
                            //clsStaticInfo obj = new clsStaticInfo();
                            obj.SaveDataSets(dsMaster);
                        } 
                    }

                }

                catch (Exception ex)
                {
                    throw ex;
                }


                //return DailyAllowanceAdditionalPolicyId;
                return Json(new { Message = AplosMessage.Success });
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public void SaveDailyAllowanceLeaveType(List<DailyAllowanceLeaveType> LeaveList, string DailyAllowanceAdditionalPolicyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsLeaveType;
            try
            {
                string sql1 = "DELETE FROM [dbo].[DailyAllowanceLeaveType] WHERE DailyAllowanceAdditionalPolicyId='" + DailyAllowanceAdditionalPolicyId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsLeaveType, false, "1");
                foreach (var item in LeaveList)
                {
                    string sql = "SELECT * FROM [dbo].[DailyAllowanceLeaveType] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyAllowanceLeaveType", out sID);
                        dr["ID"] = "ALT" + sID;

                        dr["DailyAllowanceAdditionalPolicyId"] = DailyAllowanceAdditionalPolicyId;
                        dr["LeaveTypeId"] = item.LeaveId;
                        dr["IsPreApplied"] = item.IsPreApplied;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        //dr["AttdnBonusPmtPolicyMasterId"] = MasterId;
                        dr["DailyAllowanceAdditionalPolicyId"] = DailyAllowanceAdditionalPolicyId;
                        dr["LeaveTypeId"] = item.LeaveId;
                        dr["IsPreApplied"] = item.IsPreApplied;
                        dr.EndEdit();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion
        #endregion -- Operations

    }

    //public class xDailyAllowance
    //{
    //    public bool CheckBoxSelect { get; set; }
    //    public string ShiftId { get; set; }
    //    public string UserName { get; set; }
    //    public bool IsActive { get; set; }
    //    public string Time { get; set; }
    //    public string EffectiveTime { get; set; }
    //    public string FromDate { get; set; }
    //    public string ToDate { get; set; }

    //}
    public class DailyAllowanceRateModel
    {
        public bool CheckBoxSelect { get; set; }
        //public string DailyAllowanceId { get; set; }
        public string Rate { get; set; }
        public bool IsFixed { get; set; }
        public string FormulaDescription { get; set; }
        public string FormulaDesID { get; set; }
        public string DesignationId { get; set; }


    }
    public class DailyAllowanceRateBasedOnSalaryRangeModel
    {
        public bool CheckBoxSelect { get; set; }
        public string DailyAllowanceId { get; set; }
        public string Rate { get; set; }

        public string SalaryRangeUpperLimit { get; set; }
        public string SalaryRangeLowerLimit { get; set; }
        public string SalaryHeadId { get; set; }
        public string SalaryHead { get; set; }

    }
    public class AllowanceDailyModel : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string PlantId { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string SalaryHeadId { get; set; }
        public bool Active { get; set; }
        public string FormulaDescription { get; set; }
        public string FormulaDesID { get; set; }
        public string CalculationBasics { get; set; }
        public string Catagory { get; set; }
        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        public bool IsAllDesignation { get; set; }
        public bool IsFixed { get; set; }
        public string Rate { get; set; }
        public bool IsAllShift { get; set; }
        public string EffectiveTime { get; set; }
        public bool IsSpecificTime { get; set; }
        public string FromEffectiveDate { get; set; }
        public string ToEffectiveDate { get; set; }
        public bool IsRateBasedOnSalaryRange { get; set; }
        public string SalaryRangeBasedOnSalaryHeadId { get; set; }
        public bool IsVoucherPayment { get; set; }
        #endregion
    }
}