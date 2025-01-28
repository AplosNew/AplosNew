#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;
using Library.Model.Enums;
using Library.Service.Helpers;

#endregion Using

namespace Aplos.Areas.Farming.Controllers
{
    public class VoucherController : BaseController
    {
        string TableName = "dbo.Voucher";
        string TableName1 = "dbo.VoucherChild";
        string TableName2 = "TRN.PurchaseBookingSoda";

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public VoucherController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       


     
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM "+ TableName +"  "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult geticsmaster()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,Name AS Text FROM [MST].[ICSMaster]"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetVoucherList(string FromDate, string ToDate, string ICSMasterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"select distinct fm.*,ics.Id as ICSMasterID ,ics.Name as ICSMaster, cp.Id as CPId, cp.UserName as CropPlanning, pbs.Id as PBSId,pbs.Date as PBSDate,pbs.IsPayment,pbs.IsVoucher, TA.TotalAmount, kk.TotalTransaction
                                                         from MST.FarmerMaster fm left join MST.FarmerMasterPlot fmp on fm.Id=fmp.FarmerMasterId
														 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
														 left join TRN.CropPlanning cp on cp.ICSMasterID=fmp.ICSMasterId
														 left join TRN.PurchaseBookingSoda pbs on cp.Id=pbs.CropPlanningId
														 left join (
													     select COUNT(PurchaseBookingSodaMasterId) as TotalTransaction,PurchaseBookingSodaMasterId 
														 FROM TRN.PurchaseBookingSodaChild group by PurchaseBookingSodaMasterId
													) kk on kk.PurchaseBookingSodaMasterId=pbs.Id
													left join (
													     select SUM(PaymentQuantity * PaymentRate) as TotalAmount,PurchaseBookingSodaMasterId
														 FROM TRN.PurchaseBookingSodaChild group by PurchaseBookingSodaMasterId
													) TA on TA.PurchaseBookingSodaMasterId=pbs.Id
													
													where fmp.ICSMasterId='" + ICSMasterID + @"' and pbs.IsPayment=1 and pbs.IsVoucher=0 and (pbs.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '"+ ToDate + @"')) ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult getvouchergeneratedlist(string Id)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"select distinct v.Id,FORMAT(v.Date,'dd-MMM-yyyy') as VoucherDate,CONVERT(varchar(5),v.[Time],108)[VoucherTime],v.PreparedById, v.ApprovedById, Fm.Id as FmId, Fm.FarmerName, fm.FarmerRegistrationID, fm.MobileNo,
                                                         fm.NationalID, fm.BankName, fm.AccountNo, fm.IFSCCode, fm.DebitGLCode, fm.CreditGLCode, fm.Remarks, ics.Name as ICSMaster,TA.TotalAmount, kk.TotalTransaction
														 ,EI.EmployeeCode, EI.EmployeeName, EI.EmployeeStatus,EmpI.EmployeeCode as EmpCode, EmpI.EmployeeName as EmpName, EmpI.EmployeeStatus as EmpStatus
														 from dbo.Voucher v left join dbo.VoucherChild vc on v.Id=vc.VoucherMasterId
														 left join MST.FarmerMaster fm on fm.Id=vc.FarmerMasterId and vc.VoucherMasterId=v.Id
														 left join MST.FarmerMasterPlot fmp on fm.Id=fmp.FarmerMasterId
														 left join MST.ICSMaster ics on ics.Id=fmp.ICSMasterId
														 left join TRN.CropPlanning cp on cp.ICSMasterID=fmp.ICSMasterId
														 left join TRN.PurchaseBookingSoda pbs on cp.Id=pbs.CropPlanningId
														 left join dbo.EmployeeInformation EI on EI.SystemId=v.PreparedById
														 left join dbo.EmployeeInformation EmpI on EmpI.SystemId=v.ApprovedById
														 left join (
													     select COUNT(PurchaseBookingSodaMasterId) as TotalTransaction,PurchaseBookingSodaMasterId 
														 FROM TRN.PurchaseBookingSodaChild group by PurchaseBookingSodaMasterId
													) kk on kk.PurchaseBookingSodaMasterId=pbs.Id
													left join (
													     select SUM(PaymentQuantity * PaymentRate) as TotalAmount,PurchaseBookingSodaMasterId
														 FROM TRN.PurchaseBookingSodaChild group by PurchaseBookingSodaMasterId
													) TA on TA.PurchaseBookingSodaMasterId=pbs.Id
													where pbs.IsPayment=1 and pbs.IsVoucher=1 and v.Id='" + Id + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.Voucher where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           

            string sql = @"select top 100 * from (select distinct v.*,FORMAT(v.Date,'dd-MMM-yyyy') as VoucherDate,CONVERT(varchar(5),v.[Time],108)[VoucherTime],EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson, EmpI.EmployeeStatus as EmpStatus,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName
                                                        from dbo.Voucher v left join dbo.EmployeeInformation EI on v.PreparedById=EI.SystemId
														left join dbo.EmployeeInformation EmpI on v.ApprovedById=EmpI.SystemId
											      ) AS TEMP WHERE " + strkey + " order by Date desc ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, IEnumerable<VoucherChild> VoucherChildData, string IsVoucherData)
        {
            try
            {
                DataSet dsMaster;
             
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "V" + _Id;
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
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                SaveVoucherChild(VoucherChildData, MasterId, IsVoucherData);

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



        // *************** Voucher Child ***************************

        [HttpPost, Authorize]
        public JsonResult SaveVoucherChild(IEnumerable<VoucherChild> VoucherChildData, string MasterId, string IsVoucherData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet dsMaster;
                DataSet dsMaster1;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                //con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                foreach (var item in VoucherChildData)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where FarmerMasterId='" + item.Id + "' and VoucherMasterId='"+ MasterId + "'  ", out dsMaster, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = GetVCPK();

                        dr["VoucherMasterId"] = MasterId;
                        dr["FarmerMasterId"] = item.Id;
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

                        dr.BeginEdit();

                        dr["VoucherMasterId"] = MasterId;
                        dr["FarmerMasterId"] = item.Id;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where Id='" + item.PBSId + "' and CropPlanningId='"+ item.CPId + "' and IsPayment='"+ item.IsPayment +"' ", out dsMaster1, false, "1");
                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        //edit
                        DataRow dr = dsMaster1.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        dr["IsVoucher"] = IsVoucherData;
                        dr["VoucherId"] = MasterId;
                        dr["VoucherDate"] = System.DateTime.Now.ToString();

                        dr.EndEdit();
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster, dsMaster1);
                }

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        [Authorize]
        private string GetVCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(VoucherChild), out sID);
            return sID;
        }

        // Employee Responsible Person field
        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetailsForSelection(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"
                        SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(PreparedById,'') from dbo.Voucher where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        // Approve By field
        [HttpPost, Authorize]
        public ActionResult LoadAllEmpApproveByDetailsForSelection(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"
                        SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ApprovedById,'') from dbo.Voucher where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }

    public class VoucherChild : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string VoucherMasterId { get; set; }


        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string FarmerMasterId { get; set; }
        public string CPId { get; set; }
        public string PBSId { get; set; }
        public string IsPayment { get; set; }
        public string IsVoucher { get; set; }
        //public string TargetRatee { get; set; }
        //public string DCRId { get; set; }
        //public string BalanceBook { get; set; }
        //public string BalancePurchase { get; set; }
        //public string Remarks { get; set; }
        //public string Amount { get; set; }
        //public string ConfirmedQuantity { get; set; }
        //public string ConfirmedRate { get; set; }
        //public string ApproveQuantity { get; set; }
        //public string ApproveRate { get; set; }
        //public string PaidQuantity { get; set; }
        //public string PaidRate { get; set; }

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

}