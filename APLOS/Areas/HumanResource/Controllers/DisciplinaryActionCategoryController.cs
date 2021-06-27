#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class DisciplinaryActionCategoryController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public DisciplinaryActionCategoryController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region Constructor

        private readonly IDisciplinaryActionCategoryService _DisciplinaryActionCategoryService;

        public DisciplinaryActionCategoryController(IDisciplinaryActionCategoryService DisciplinaryActionCategoryService
            )
        {
            _DisciplinaryActionCategoryService = DisciplinaryActionCategoryService;
        }

        #endregion Constructor
              
        public ActionResult Aplos()
        {
            return View();
        }
        
        [HttpGet,Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,Sequence,Code,ShortName,StandardName,UserName,Active,Description from [HKP].[DisciplinaryActionCategory]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetChildData(string DetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,DisciplinaryActionSettingDetailsId,DisciplinaryActionCategoryId,LetterFormat,LetterName,LetterLanguage,IsDefault,IsActive
                                        from DisciplinaryActionSettingChild where DisciplinaryActionSettingDetailsId='" + DetailsId+@"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetShowCaseLetter(string DisciplinaryActionCategoryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                            select DAS.Id,DAS.DisciplinaryActionCategoryId,DAS.Sequence,DAS.LetterIssueDay,DAS.IsSeparable,DAS.Description ,das.IsActive
                            ,Separable=case when DAS.IsSeparable=1 then 'Yes' else 'No' END 
                            ,Active=case when DAS.IsActive=1 then 'Yes' else 'No' END 
                            From  DisciplinaryActionSettingDetails  DAS					
							where das.DisciplinaryActionCategoryId='" + DisciplinaryActionCategoryId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT MAX(Sequence)+1 AS Sequence FROM [HKP].[DisciplinaryActionCategory]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveM(DisciplinaryActionCategory Master)
        {
            try
            {
                string MasterId = string.Empty;
                MasterId = SaveMaster(Master);
                return Json(new { MasterId, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Authorize]
        public string SaveMaster(DisciplinaryActionCategory Master)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;
                string sql = "SELECT * FROM [HKP].[DisciplinaryActionCategory] WHERE ID='" + Master.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[HKP].[DisciplinaryActionCategory]", out sID);
                    Id = "DACM" + sID;
                    dr["Id"] = Id;
                    dr["Sequence"] = Master.Sequence;
                    dr["Code"] = Master.Code;
                    dr["ShortName"] = Master.ShortName;
                    dr["StandardName"] = Master.StandardName;
                    dr["UserName"] = Master.UserName;                
                    dr["Remarks"] = Master.Remarks;
                    dr["Active"] = Master.Active;
                    dr["Archive"] = false;
                    dr["Description"] = Master.Description;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();

                    dr["Sequence"] = Master.Sequence;
                    dr["Code"] = Master.Code;
                    dr["ShortName"] = Master.ShortName;
                    dr["StandardName"] = Master.StandardName;
                    dr["UserName"] = Master.UserName;                    
                    dr["Remarks"] = Master.Remarks;
                    dr["Active"] = Master.Active;
                    dr["Description"] = Master.Description;
                    dr["Archive"] = false;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult SaveD(DisciplinaryActionCategoryDetails Details, string MasterId)
        {
            try
            {
                if (MasterId == null)
                {
                    throw new Exception("Save Disciplinary Action Category First...");
                }
                string DetailsId = string.Empty;
                DetailsId = SaveDetails(Details, MasterId);
                return Json(new { DetailsId, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public string SaveDetails(DisciplinaryActionCategoryDetails Details, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;

            try
            {
                string DetailsId = string.Empty;

                string sql = "SELECT * FROM [dbo].[DisciplinaryActionSettingDetails] WHERE ID='" + Details.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[DisciplinaryActionSettingDetails]", out sID);
                    dr["Id"] = "DASD" + sID;
                    DetailsId = dr["Id"].ToString();

                    dr["DisciplinaryActionCategoryId"] = MasterId;
                    dr["Sequence"] = Details.Sequence;
                    dr["LetterIssueDay"] = Details.LetterIssueDay;
                    dr["IsSeparable"] = Details.IsSeparable;
                    dr["PlantId"] = identity.PlantId;
                    dr["Description"] = Details.Description;
                    dr["IsActive"] = Details.IsActive;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    DetailsId = dr["Id"].ToString();

                    dr["DisciplinaryActionCategoryId"] = MasterId;
                    dr["Sequence"] = Details.Sequence;
                    dr["LetterIssueDay"] = Details.LetterIssueDay;
                    dr["IsSeparable"] = Details.IsSeparable;
                    dr["PlantId"] = identity.PlantId;
                    dr["Description"] = Details.Description;
                    dr["IsActive"] = Details.IsActive;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return DetailsId;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult SaveC(DisciplinaryActionSettingChild Child, string DetailsId, string MasterId)
        {
            try
            {               
                SaveChildren(Child, DetailsId, MasterId);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveChildren(DisciplinaryActionSettingChild Child, string DetailsId,string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;
                string sql = "SELECT * FROM DisciplinaryActionSettingChild WHERE ID='" + Child.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (Child.IsDefault == true)
                {
                    DataSet dsDefaultValidation;
                    string sql1 = "select * from DisciplinaryActionSettingChild where DisciplinaryActionSettingDetailsId='" + DetailsId + @"' and IsDefault=1 and Id<> '"+ Child.Id+ @"' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql1, out dsDefaultValidation, false, "1");
                    if (dsDefaultValidation.Tables[0].Rows.Count > 0)
                    {
                        Exception ex = new Exception("Default Letter Can't More Then One...");
                        throw (ex);
                    }
                }

                if (dsMaster.Tables[0].Rows.Count == 0)
                {                   
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    AddEdit("ADD", ref dr, Child, MasterId, DetailsId);
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    AddEdit("EDIT",ref dr, Child, MasterId, DetailsId);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void AddEdit(string flag,ref DataRow dr, DisciplinaryActionSettingChild Child,string MasterId, string DetailsId)
        {
            try
            {
                if (flag.ToUpper() == "ADD")
                {
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DisciplinaryActionSettingChild", out sID);
                    dr["Id"] = "DACC" + sID;
                    dr["DisciplinaryActionSettingDetailsId"] = DetailsId;
                    dr["DisciplinaryActionCategoryId"] = MasterId;
                    dr["LetterFormat"] = Child.LetterFormat;
                    dr["LetterName"] = Child.LetterName;
                    dr["LetterLanguage"] = Child.LetterLanguage;
                    dr["IsDefault"] = Child.IsDefault;
                    dr["IsActive"] = Child.IsActive;
                }
                else
                {
                    dr.BeginEdit();

                    dr["DisciplinaryActionSettingDetailsId"] = DetailsId;
                    dr["DisciplinaryActionCategoryId"] = MasterId;
                    dr["LetterFormat"] = Child.LetterFormat;
                    dr["LetterName"] = Child.LetterName;
                    dr["LetterLanguage"] = Child.LetterLanguage;
                    dr["IsDefault"] = Child.IsDefault;
                    dr["IsActive"] = Child.IsActive;

                    dr.EndEdit();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        public ActionResult Delete(string Id)
        {
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsDetailsChild;
            DataSet dsDetails;
            try
            {
                string sql = "select * from DisciplinaryActionSettingChild where DisciplinaryActionCategoryId='"+Id+@"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDetailsChild, false, "1");
                if (dsDetailsChild.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Please Delete Letter Details First ....");
                    throw (ex);
                }

                string sql1 = "select * from  [dbo].[DisciplinaryActionSettingDetails]  where DisciplinaryActionCategoryId='"+Id+@"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsDetails, false, "1");
                if (dsDetails.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Please Delete Letter Details First ....");
                    throw (ex);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM  [HKP].[DisciplinaryActionCategory] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DeleteDetails(string Id)
        {
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsDetails;
            try
            {
                string sql = "select * from DisciplinaryActionSettingChild where DisciplinaryActionSettingDetailsId='"+Id+@"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDetails, false, "1");
                if (dsDetails.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Please Delete Letter Details First ....");
                    throw (ex);
                }
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM  [dbo].[DisciplinaryActionSettingDetails]  WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DeleteChild(string Id)
        {
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsChild;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM  DisciplinaryActionSettingChild WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsChild, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLanguage()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select UserName,Id From [SCS].[Language] order by UserName asc";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class DisciplinaryActionCategory : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public decimal Sequence { get; set; }
            public string Code { get; set; }
            public string ShortName { get; set; }
            public string StandardName { get; set; }
            public string UserName { get; set; }
            public string Description { get; set; }
            public string Remarks { get; set; }
            public bool Active { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }
            #endregion Audit Properties
        }

        public class DisciplinaryActionCategoryDetails : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string DisciplinaryActionCategoryId { get; set; }
            public string Sequence { get; set; }
            public string LetterIssueDay { get; set; }
            public bool IsSeparable { get; set; }
            public string PlantId { get; set; }
            public string Description { get; set; }
            public bool IsActive { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class DisciplinaryActionSettingChild : BaseModel
        {
            public string Id { get; set; }
            public string DisciplinaryActionSettingDetailsId { get; set; }
            public string DisciplinaryActionCategoryId { get; set; }
            public string LetterFormat { get; set; }
            public string LetterName { get; set; }
            public string LetterLanguage { get; set; }
            public bool IsDefault { get; set; }
            public bool IsActive { get; set; }
        }

    }
}