#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Properties;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Taxations;

using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;


#endregion Using

namespace Aplos.Areas.Farming.Controllers
{
    public class ICSMasterController : BaseController
    {
        string TableName = "MST.ICSMaster";


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ICSMasterController(ISqlRepository R)
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
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + "  "), JsonRequestBehavior.AllowGet);
        }




        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from MST.ICSMaster where Id = '" + Id + "' ");


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


            string sql = @"select top 100 * from (select distinct icsm.*,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,e.UserName as EntityName,p.UserName as PlantName,p.Id as PlantId, c.UserName as CompanyName,c.Id as CompanyId
                                                 from MST.ICSMaster icsm left join dbo.EmployeeInformation EI on icsm.ResponsiblePersonId=EI.SystemId
												 left join ORG.Entity e on icsm.EntityId=e.Id
												 left join ORG.Plant p on e.PlantId=p.Id
                                                 left join ORG.Company c on p.CompanyId=c.Id) AS TEMP WHERE " + strkey + " order by RegistrationCode ";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            var model = new JavaScriptSerializer().Deserialize<ICSMaster>(form["ICSMaster"]);



            SaveData(model);
            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetICSMasterDocumentPath();
                var path = Path.Combine(directory);



                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }



                var fileId = "";
                var fileName = "";
                var filedata = GetFile(model.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["Id"].ToString();
                    fileName = filedata["FileName"].ToString();



                    if (fileName != model.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }



                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + Path.GetExtension(item.FileName));
                        item.SaveAs(path + Path.GetFileNameWithoutExtension(item.FileName) + Path.GetExtension(item.FileName));
                    }
                }



            }
            return Json(new { Message = AplosMessage.Success });
        }


        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ICSMaster), out sID);
            return sID;
        }

        private void SaveData(ICSMaster data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {
                //GetAutoSequence(data.Id, out dsSeq);
                //decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());

                string sql = "SELECT * FROM [MST].[ICSMaster] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "IC" + GetPK();
                    dr["RegistrationCode"] = data.RegistrationCode;
                    dr["Group"] = data.Group;
                    dr["Name"] = data.Name;
                    dr["LicenseNumber"] = data.LicenseNumber;
                    dr["RegistrationID"] = data.RegistrationID;
                    dr["RegistrationDate"] = data.RegistrationDate;
                    dr["RenewalPeriod"] = data.RenewalPeriod;
                    dr["UserInfo1"] = data.UserInfo1;
                    dr["UserInfo2"] = data.UserInfo2;
                    dr["Remarks"] = data.Remarks;
                    dr["ResponsiblePersonId"] = data.ResponsiblePersonId;
                    dr["FileName"] = data.FileName;
                    dr["DebitGL"] = data.DebitGL;
                    dr["CreditGL"] = data.CreditGL;
                    dr["EntityId"] = data.EntityId;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);

                    contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["RegistrationCode"] = data.RegistrationCode;
                    dr["Group"] = data.Group;
                    dr["Name"] = data.Name;
                    dr["LicenseNumber"] = data.LicenseNumber;
                    dr["RegistrationID"] = data.RegistrationID;
                    dr["RegistrationDate"] = data.RegistrationDate;
                    dr["RenewalPeriod"] = data.RenewalPeriod;
                    dr["UserInfo1"] = data.UserInfo1;
                    dr["UserInfo2"] = data.UserInfo2;
                    dr["Remarks"] = data.Remarks;
                    dr["ResponsiblePersonId"] = data.ResponsiblePersonId;
                    dr["FileName"] = data.FileName;
                    dr["DebitGL"] = data.DebitGL;
                    dr["CreditGL"] = data.CreditGL;
                    dr["EntityId"] = data.EntityId;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
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
        }

        public Dictionary<string, object> GetFile(string Id)
        {
            try
            {
                var sql = @"SELECT Id, FileName FROM [MST].[ICSMaster]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public ActionResult Delete(string id)
        {
            string sql = @"select * from [MST].[ICSMaster] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");


                // ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

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
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from MST.ICSMaster where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


    }

    public class ICSMaster : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string RegistrationCode { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public string LicenseNumber { get; set; }
        public string RegistrationID { get; set; }
        public string RegistrationDate { get; set; }
        public string RenewalPeriod { get; set; }
        public string UserInfo1 { get; set; }

        public string UserInfo2 { get; set; }

        public string Remarks { get; set; }

        public string ResponsiblePersonId { get; set; }

        public string FileName { get; set; }

        public string DebitGL { get; set; }
        public string CreditGL { get; set; }
        public string EntityId { get; set; }



        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string QMSDefectMasterId { get; set; }

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