using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;

namespace Aplos.Areas.Employees.Controllers
{
    /// <summary>
    /// <remark>Author:Mehedi Hasan Tamim;Date:30-12-2015;</remark>
    /// <remark>Modified:Belayet Hossain;Date:6-Jan-2016;</remark>
    /// </summary>
    public class PaymentModeChangeController : BaseController
    {
        #region Constructor
        /// <summary>   The separationTypeService service. </summary>
        private readonly ISeparationTypeService _separationTypeService;
        private readonly ISqlRepository _sqlRepository;
        public PaymentModeChangeController(ISeparationTypeService separationTypeService, ISqlRepository sqlRepository)
        {
            _separationTypeService = separationTypeService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet]
        public ActionResult LoadEmployeelist(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT 0 CheckBoxSelect, EI.SystemId
                         ,EI.EmployeeCode
                         ,EI.EmployeeName,ei.EmployeeCodePreFix,ei.EmployeeCodeNumeric
                         , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                         , DG.UserName LegalDesignation
                         , DP.UserName Department
                         , PMB.Code,PR.UserName PositionName
                        
                         , EI.PaymentMode,EI.EmployeeStatus
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId				
                         WHERE  EI.PlantId='" + identity.PlantId + @"' AND ( EI.DOJ<='"+ToDate+ @"' AND ( EI.dos IS NULL  OR EI.DOS>='" + FromDate + @"' ))
                         ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet,Authorize]
        public ActionResult GetPaymentMode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"SELECT * FROM  PaymentMode ";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }




        

        

        [HttpPost]
        public JsonResult SaveChangeData(string[] EmployeeSystemIdList, string EmployeePaymentMode)
        {
            string EmpId = string.Empty;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsEmplistList=null;
           
            try
            {
                if (EmployeeSystemIdList.Length>0)
                {
                    for (int i = 0; i < EmployeeSystemIdList.Length; i++)
                    {
                        if (EmpId == "")
                            EmpId = "'" + EmployeeSystemIdList[i].ToString() + "'";
                        else
                            EmpId = EmpId + ",'" + EmployeeSystemIdList[i].ToString() + "'";
                    }

                    string sql = @"SELECT * FROM Employeeinformation WHERE PlantID = '" + identity.PlantId + "' and SystemId IN (" + EmpId + @")";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsEmplistList, false, "1");
                    Dictionary<string, DataRow> DicEmplistList = new Dictionary<string, DataRow>();
                    for (int i = 0; i < dsEmplistList.Tables[0].Rows.Count; i++)
                    {
                        DicEmplistList.Add(dsEmplistList.Tables[0].Rows[i]["SystemId"].ToString(), dsEmplistList.Tables[0].Rows[i]);
                    }

                   


                    for (int i = 0; i < EmployeeSystemIdList.Length; i++)
                    {


                        if (DicEmplistList.ContainsKey(EmployeeSystemIdList[i].ToString()) == true)
                        {
                            DataRow dr = DicEmplistList[EmployeeSystemIdList[i].ToString()];
                            dr.BeginEdit();
                            dr["PaymentMode"] = EmployeePaymentMode.ToString();
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                    }









                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsEmplistList);
                }             

                
                


            }
            catch (Exception ex)
            {

                throw (ex);
            }













            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // _restService.Insert(rest, identity.PlantId, restDetails);
            return Json(new { Message = AplosMessage.Success });
        }

        

        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName  FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmploymentTypelistForFiexdDays()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT UserName EmploymentType,'' DayNo FROM EmploymentTypeEnum  ";


            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT max(Sequence)+1 Sequence FROM hkp.[SeparationType] WHERE PlantID='"+ identity .PlantId+ "'";          

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


      
        #endregion
    }
    public class xSeparationTypeDetails
    {
        public string Id { get; set; }
        public string YearNo { get; set; }
        public string DayNo { get; set; }
        public bool RoundUp { get; set; }
        public string EmploymentType { get; set; }
    }

   
}