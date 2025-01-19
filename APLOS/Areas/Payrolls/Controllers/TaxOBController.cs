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
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Tax;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Web;
using Aplos.Helpers;
using System.IO;
using Library.Service.Helpers;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class TaxOBController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeProfileService _employeeProfileService;

        public TaxOBController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region View

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region -- Get --
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeeProfileService.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxYear()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetCompTaxYear(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTabValue(string Doj, string TaxYeadId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTab(Doj, TaxYeadId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxType()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetIncomeTaxType(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetIncomeTaxTransaction(string TaxYear, string TaxType, string empId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetIncomeTaxTransactionInv(TaxYear, TaxType, empId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxableIncomePara(string TaxYear, string TaxType, string empId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetTaxableIncomePara(TaxYear, TaxType, empId, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetIncomeTabValue(string TaxYear, string TaxType, string empId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetIncomeTabValue(TaxYear, TaxType, empId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetIncomeTaxTransactionDed(string TaxYear, string TaxType, string empId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IncomeTaxPolicy ep = new IncomeTaxPolicy();
                return Json(ep.GetIncomeTaxTransactionDed(TaxYear, TaxType, empId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string TaxYear, string TaxType, string empid)
        {
            try
            {
                if (string.IsNullOrEmpty(TaxYear))
                {
                    throw new Exception("No Tax Year is selected...");
                }
                if (string.IsNullOrEmpty(TaxType))
                {
                    throw new Exception("No Tax Type is selected...");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                TaxOB ep = new TaxOB();
                return Json(ep.GetList(TaxYear, TaxType, empid), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

        #region -- Save --
        [HttpPost]
        public JsonResult Create(EmpLists EmpList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //EmpList.AddedBy = identity.Name;
                TaxOB ep = new TaxOB();
                ep.SaveMaster(EmpList);
                return Json(new { Error = false, Data = EmpList, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [Authorize, HttpPost]
        public JsonResult SaveInvestment(IncomeTaxItemTransaction Investment, List<IncTaxItmChild> ChildList)
        {
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(Investment.EmpSystemID))
                {
                    throw new Exception("Select Employee ..");
                }
                if (string.IsNullOrEmpty(Investment.TaxTypeId))
                {
                    throw new Exception("Select Tax Type..");
                }
                if (string.IsNullOrEmpty(Investment.TaxYearId))
                {
                    throw new Exception("Select Tax Year");
                }
                List<IncTaxItmChild> SaveList = new List<IncTaxItmChild>();
                for (int i = 0; i < ChildList.Count; i++)
                {
                    if (ChildList[i].IsSelect)
                    {
                        if (clsStaticInfo.dbl(ChildList[i].Value) <= 0)
                        {
                            throw new Exception("Selected Line Item must have value..");
                        }
                        else
                        {
                            //SaveList.Add(ChildList[i]);
                        }
                    }
                    if (ChildList[i].IsSelect == false && ChildList[i].Value > 0)
                    {
                        //throw new Exception("Select the Value provided line item..");
                    }
                }
                if (ChildList.Count == 0)
                {
                    throw new Exception("Nothing to Update..");
                }

                ChildList = ChildList.OrderBy(w => w.GroupId).ToList();


                string _tempGroup = "";
                double GroupTotalVallue = 0;
                for (var i = 0; i < ChildList.Count; i++)
                {
                    if (i > 0)
                    {
                        if (_tempGroup != ChildList[i].GroupId)
                        {
                            if (GroupTotalVallue > clsStaticInfo.dbl(ChildList[i].MaxLimit))
                                throw new Exception("Total group value Cannot be greater than Tax group amount Limit");
                            GroupTotalVallue = 0;
                        }
                    }
                    if (clsStaticInfo.dbl(ChildList[i].TaxSavingItemLimit) < clsStaticInfo.dbl(ChildList[i].Value))
                    {
                        throw new Exception("Value Cannot be greater than Tax Saving Item Limit");
                    }
                    GroupTotalVallue += clsStaticInfo.dbl(ChildList[i].Value);

                    _tempGroup = ChildList[i].GroupId;
                }

                if (GroupTotalVallue > clsStaticInfo.dbl(ChildList[ChildList.Count - 1].MaxLimit))
                    throw new Exception("Total Group value Cannot be greater than Tax group amount Limit");


                #endregion
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                TaxOB ep = new TaxOB();
                ep.SaveInvsmnt(Investment, ChildList);
                return Json(new { Error = false, Data = Investment, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [Authorize, HttpPost]
        public JsonResult SaveTaxableIncomeEx(IncomeTaxItemTransaction Investment, List<TaxableIncomeparameter> ChildList)
        {
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(Investment.EmpSystemID))
                {
                    throw new Exception("Select Employee ..");
                }
                if (string.IsNullOrEmpty(Investment.TaxTypeId))
                {
                    throw new Exception("Select Tax Type..");
                }
                if (string.IsNullOrEmpty(Investment.TaxYearId))
                {
                    throw new Exception("Select Tax Year");
                }
                List<TaxableIncomeparameter> SaveList = new List<TaxableIncomeparameter>();

                bool selected = false;

                foreach (TaxableIncomeparameter item in ChildList)
                {
                    if (string.IsNullOrEmpty(item.OptionBase) || item.OptionBase == "null")
                        continue;

                    var allSameOptions = ChildList.Where(e=>e.OptionBase==item.OptionBase);
                    selected = false;
                    foreach (var option in allSameOptions)
                    {
                        if (option.IsSelect)
                            selected = true;
                    }
                    if (selected == false)
                        throw new Exception("Please select any option from "+ item.OptionBase + "");

                }

                for (int i = 0; i < ChildList.Count; i++)
                {
                    if (ChildList[i].IsSelect)
                    {
                        SaveList.Add(ChildList[i]);
                    }
                }
                if (SaveList.Count == 0)
                {
                    throw new Exception("Nothing to Update..");
                }
                #endregion
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                TaxOB ep = new TaxOB();
                ep.SaveTaxableIncomeEx(Investment, SaveList);
                return Json(new { Error = false, Data = Investment, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public IEnumerable<object> GetEmployeeList(string plantId, string companyId)
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,FORMAT(ob.CutOffDate,'dd-MMM-yyyy')CutOffDate,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LGD.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ
										,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,Emp.GenderID,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,dm.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EMP.LegalDesignationId
										LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
										left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=dm.DesignationId
										Left Join SCS.OpeningBalanceCutOffDate ob on ob.PlantId = EMP.PlantId and ob.ModuleName = 'HR'
                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' 
                                        ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion

        #region

        [HttpPost, Authorize]
        public ActionResult UploadAttachment(IEnumerable<HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\\", "");
                DataTable AdditionalData = CustomJsonResult.ToDataTable(UploadDefault_data);


                AdditionalData.Rows[0]["Id"] = AdditionalData.Rows[0]["Id"].ToString().Replace("\"", "");
                if (string.IsNullOrEmpty(AdditionalData.Rows[0]["Id"].ToString()))
                    throw new Exception("Save the item first");



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                foreach (var file in UploadDefault)
                {

                    string _Id = "IncomeTax_" + AdditionalData.Rows[0]["Id"].ToString();

                    var fileName = Path.GetFileName(_Id + new FileInfo(file.FileName).Extension);
                    var destinationPath = Path.Combine(ResourcesPathReader.TaxOpeningBalancePath(), _Id + new FileInfo(file.FileName).Extension);

                    if (System.IO.Directory.Exists(ResourcesPathReader.TaxOpeningBalancePath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.TaxOpeningBalancePath());
                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "select* from " + AdditionalData.Rows[0]["TableName"] + " where Id='" + AdditionalData.Rows[0]["Id"].ToString() + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();




                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        #region Task data update
                        if (dsLocal.Tables[0].Rows[0]["FileName"].ToString() != "")
                        {
                            //try to delete the existing file
                            try
                            {
                                var _Path = Path.Combine(ResourcesPathReader.GetToDoPath(), dsLocal.Tables[0].Rows[0]["FileName"].ToString());
                                if (System.IO.File.Exists(_Path))
                                    System.IO.File.Delete(_Path);
                            }
                            catch (Exception)
                            {

                            }

                        }

                        DataRow dr = dsLocal.Tables[0].Rows[0];

                        dr.BeginEdit();

                        dr["FileName"] = fileName;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();


                        #endregion data update





                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id /*taxyear, string taxtype,string empsysteid*/)
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("select FileName from IncomeTaxItemTransaction  where Id='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetTaxableIncomeFileInfo(string Id )
        {

            try
            {
                return Json(_sqlRepository.GetDataCollection("select FileName from TaxableIncomeparameter  where Id='" + Id + "' "), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult DeleteFile(string Id, string TableName)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + "  where Id='" + Id + "'", out dsMaster, false, "1");



                var destinationPath = Path.Combine(ResourcesPathReader.TaxOpeningBalancePath(), dsMaster.Tables[0].Rows[0]["FileName"].ToString());
                if (System.IO.File.Exists(destinationPath))
                    System.IO.File.Delete(destinationPath);

                #region Task data update


                DataRow dr = dsMaster.Tables[0].Rows[0];
                dr.BeginEdit();

                dr["FileName"] = DBNull.Value;
                dr.EndEdit();


                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new
                {
                    Error = false,
                    Message = AplosMessage.Updated
                });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        #endregion
    }
}