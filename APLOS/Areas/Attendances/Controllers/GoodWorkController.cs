#region Using

using Aplos.Controllers;
using Aplos.MaterialManagement.MaterialQuery;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class GoodWorkController : BaseController
    {
        #region Constructor
        string TableName = "HKP.TaskAppliedOn";
        private readonly ISqlRepository _sqlRepository;
        private readonly AccountVoucherReportService _accountVoucherReportService;
        clsSales clsSales = new clsSales();
        public GoodWorkController(ISqlRepository R, AccountVoucherReportService accountVoucherReportService)
        {
            _sqlRepository = R;
            _accountVoucherReportService = accountVoucherReportService;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        //Good Work

        [HttpGet, Authorize]
        public JsonResult GetEmployeeListByPlant(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_accountVoucherReportService.EmployeeListByPayable(parameters, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetGoodWorkDataList(string empId)
        {
            string sql = @"SELECT Emp.SystemId,EMP.EmployeeName,EMP.EmployeeCode,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            WHERE  EMP.SystemId= '" + empId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllActiveEmployeeData()
        {
            JsonResult json = Json(clsSales.GetAllEmployeeData(), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpPost]
        public ActionResult GetMinute(GoodWorkTransaction data)
        {
            var ts = data.ToTime.Subtract(data.FromTime);
            return Json(ts.TotalMinutes, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult GetGoodWorkcenter()
        //{
        //    string str = @"SELECT '' CalculatedTime FROM SCS.WorkCenterMaster";
        //    return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        //}
        public class GoodWorkTransaction
        {
            #region Scalar Properties

            public DateTime FromTime { get; set; }
            public DateTime ToTime { get; set; }
            public int Minute { get; set; }
      
            #endregion Scalar Properties

            #region Audit Properties

            /// <summary>
            ///This is  AddedBy.Who add data keep track by AddedBy.
            /// </summary>
            [NeverUpdate]
            public string AddedBy { get; set; }

            /// <summary>
            ///This is  AddedDate.Added date keep track by AddedDate.
            /// </summary>
            [NeverUpdate]
            public DateTime AddedDate { get; set; }

            /// <summary>
            /// Record insert by user from IP address.
            /// </summary>
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            /// <summary>
            /// Record updated user name.
            /// </summary>
            public string UpdatedBy { get; set; }

            /// <summary>
            /// Record updated by user date and time.
            /// </summary>
            public DateTime? UpdatedDate { get; set; }

            /// <summary>
            /// Record updated by user IP address.
            /// </summary>
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        //Good Work
        [HttpPost, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM " + TableName;
            DataTable dt = _sqlRepository.GetDataTable(sql);
            foreach (TaskAppliedOnEnum _data in Enum.GetValues(typeof(TaskAppliedOnEnum)))
            {
                dt.DefaultView.RowFilter = "TaskAppliedOnEnum='" + _data.ToString() + "'";
                if (dt.DefaultView.Count == 0)
                {
                    DataRow dr = dt.NewRow();
                    dr["TaskAppliedOnEnum"] = _data.ToString();
                    dt.Rows.Add(dr);
                }
            }
            dt.DefaultView.RowFilter = null;

            return Json(Helpers.CustomJsonResult.DataTableToJson(dt), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(List<Dictionary<string, object>> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName, out dsMaster, false, "1");

                string _Id = "";




                #region data update

                bplib.clsGenID genid;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i]["UserName"] != null)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "TaskAppliedOnEnum='" + data[i]["TaskAppliedOnEnum"].ToString() + "'";
                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            if (_Id == "")
                            {
                                genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                            }
                            data[i]["Id"] = "A" + _Id + (i + 1).ToString();
                            AddNewRow(dsMaster.Tables[0], data[i]);
                        }
                        else
                        {

                            EditRow(dsMaster.Tables[0].DefaultView[0].Row, data[i]);
                        }
                    }

                }

                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Message = AplosMessage.Updated });

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

        #region User Edit Cotrol
        [HttpPost]
        public JsonResult CreateUserEditControl(Dictionary<string, object> data, List<Dictionary<string, object>> userECDetail)
        {
            try
            {
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                DataSet dsMaster;
                DataSet dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from UserEditControl where UserId='" + data["UserId"] + "'", out dsMaster, false, "1");
       

                string _Id = "";

                #region data update
                
                        //dsMaster.Tables[0].DefaultView.RowFilter = "TaskAppliedOnEnum='" + data[i]["TaskAppliedOnEnum"].ToString() + "'";
                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            if (_Id == "")
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("UserEditControl", out _Id);
                            }
                            data["Id"] =_Id ;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                          data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                        }

                #endregion data update

                #region User Edit Control Detail
                
                string _MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                con.OpenDataSetThroughAdapter("select * from UserEditControlDetail where UserEditControlId='" + _MasterId + "'", out dsDetail, false, "1");
                int ccount = 0;
                if (userECDetail != null)
                {
                    foreach (var item in userECDetail)
                    {
                        //string _DetailId = "";
                        //if (_DetailId == "")
                        //{
                        //    bplib.clsGenID genid = new bplib.clsGenID();
                        //    genid.GenID("UserEditControlDetail", out _DetailId);
                        //}


                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count == 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            item["Id"] = detailid;
                            item["UserEditControlId"] = _MasterId;
                            item["Href"] = item["Href"];

                            materialCommonService.AddNewRowD(dsDetail.Tables[0], item);

                        }
                        if (dv.Count > 0)
                        {
                            ccount++;
                            string detailid = materialCommonService.MakePK(_MasterId, ccount, 2);
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Id"] = detailid;
                            drmo["UserEditControlId"] = _MasterId;
                            drmo["Href"] = item["Href"]; ;
                            drmo.EndEdit();

                        }
                    }
                }

                #endregion User Edit Control Detail
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetUserEditControlList()
        {
            string sql = @"select UEC.*,U.*,UserType=Case when U.SysAdmin=1 then 'System User' else 'General User' end
                            from UserEditControl UEC
                            left join sec.[User] U on U.Id=UEC.UserId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetUserEditControlDetailList(string userEditControlId)
        {
            string sql = @"select UEC.*,UECD.Href,MM.Description,MM.Controller
                            from UserEditControl UEC
							left join UserEditControlDetail UECD on UECD.UserEditControlId=UEC.Id
							left join [MST].[MenuMaster] MM on MM.Href=UECD.Href
                            where UEC.Id = '"+ userEditControlId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.UserEditControlDetail where UserEditControlId='" + Id + "'");
                con.executeQuery("delete from dbo.UserEditControl where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteChildUrl(string Id)
        {
            try
            {
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.UserEditControlDetail where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpGet]
        public JsonResult GetHreflist(GridParameter parameters)
        {
            return Json(GetHreflistData(parameters), JsonRequestBehavior.AllowGet);
        }

        public GridModel GetHreflistData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"select Id,Description, Controller, Href from [MST].[MenuMaster]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetHrefDatasList(string hrefId)
        {
            string sql = @"select UECD.Id,UECD.UserEditControlId,MM.Description,MM.Controller,MM.Href
							from [MST].[MenuMaster] MM 
							left join UserEditControlDetail UECD on UECD.Href=MM.Href
							left join UserEditControl UEC on UEC.Id=UECD.UserEditControlId
                            where UEC.UserId = '" + hrefId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion User Edit Cotrol
    }
}