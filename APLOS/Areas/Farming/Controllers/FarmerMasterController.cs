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

using Library.Service.Helpers;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Net.Http;

using Library.Model.IE;
using Library.Service.IEnumerable;
using Library.Model.Enums;
using Syncfusion.XlsIO;

#endregion Using

namespace Aplos.Areas.Farming.Controllers
{
    public class FarmerMasterController : BaseController
    {
        string TableName = "MST.FarmerMaster";
        string TableName1 = "MST.FarmerMasterPlot";


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public FarmerMasterController(ISqlRepository R)
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
        public JsonResult getcountrylist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text from SCS.Country order by UserName"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult getstatelist(string CountryId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [SCS].[State] where CountryId='"+ CountryId + "' order by UserName"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDistrictList(string StateId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [SCS].[District] where StateId='" + StateId + "' order by UserName "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTalukList(string DistrictId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM HKP.Taluk where DistrictId='" + DistrictId + "' order by UserName "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetVillagesList(string TalukId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM HKP.Village where TalukId='" + TalukId + "' order by UserName "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetICSMasterList()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,Name AS Text FROM MST.ICSMaster"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getplotstatuslist()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName AS Text FROM HKP.LandCategory"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getplotareasum(string FarmerMasterId)
        {
            return Json(_sqlRepository.GetDataCollection("select SUM(PlotArea) as TotalArea,FarmerMasterId FROM MST.FarmerMasterPlot where FarmerMasterId='"+ FarmerMasterId + "' group by FarmerMasterId"), JsonRequestBehavior.AllowGet);
        }



        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from MST.FarmerMaster where Id = '" + Id + "' ");


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
           

            string sql = @"select top 100 * from (select fm.*,c.Id as CountryId,c.UserName as Country,s.UserName as State,d.UserName as District,t.UserName as Taluk,v.UserName as Villages,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson
                                                 from MST.FarmerMaster fm left join dbo.EmployeeInformation EI on fm.ResponsiblePersonId=EI.SystemId											
												 left join SCS.State s on fm.StateId=s.Id
												  left join SCS.Country c on c.Id=s.CountryId
												 left join SCS.District d on fm.DistrictId=d.Id
												 left join HKP.Taluk t on fm.TalukaId=t.Id
												 left join HKP.Village v on fm.VillageId=v.Id) AS TEMP WHERE " + strkey + " order by Date ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string GetFMPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FarmerMaster", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
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

                    data["Id"] = "FM" + GetFMPK();
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from [MST].[FarmerMaster] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where FarmerMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Farmer Master Plot");
                    }

                }

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
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from MST.FarmerMaster where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet]
        public JsonResult GetListFarmerMasterPlot(string FarmerMasterId)
        {

            string sql = @"select top 100 * from (select fmp.*,lc.UserName as PlotStatuss,ics.Name as ICSMaster, fmp.FileName as FileNameee from MST.FarmerMasterPlot fmp left join HKP.LandCategory lc
                                                         on fmp.PlotStatus=lc.Id
														 left join MST.ICSMaster ics on fmp.ICSMasterId=ics.id
                                                         where FarmerMasterId= '" + FarmerMasterId + "') AS TEMP order by PlotNameNo";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteFarmerMasterPlot(string Id)
        {
            try
            {
                string sql = @" delete from MST.FarmerMasterPlot where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Farmer Master Plot deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult savefarmermasterplot(FormCollection form, HttpPostedFileBase[] file)
        {
            var model = new JavaScriptSerializer().Deserialize<FarmerMasterPlot>(form["FarmerMasterPlot"]);



            SaveData(model);
            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetFarmerMasterPlotDocumentPath();
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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(FarmerMasterPlot), out sID);
            return sID;
        }

        private void SaveData(FarmerMasterPlot data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {

                string sql = "SELECT * FROM [MST].[FarmerMasterPlot] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "FMP" + GetPK();
                    dr["FarmerMasterId"] = data.FarmerMasterId;
                    dr["PlotNameNo"] = data.PlotNameNo;
                    dr["PlotArea"] = data.PlotArea;
                    dr["Survey"] = data.Survey;
                    dr["Latitude"] = data.Latitude;
                    dr["Longitude"] = data.Longitude;
                    dr["PlotStatus"] = data.PlotStatus;
                    dr["Active"] = data.Active;
                    dr["Remarks"] = data.Remarks;
                    dr["ICSMasterId"] = data.ICSMasterId;
                    dr["FarmerRegistrationID"] = data.FarmerRegistrationID;
                    dr["FarmerRegistrationDate"] = data.FarmerRegistrationDate;
                    dr["InspectionDate"] = data.InspectionDate;
                    dr["ApprovalDate"] = data.ApprovalDate;
                    dr["RenewalPeriod"] = data.RenewalPeriod;
                    dr["FileName"] = data.FileName;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);

                    contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["FarmerMasterId"] = data.FarmerMasterId;
                    dr["PlotNameNo"] = data.PlotNameNo;
                    dr["PlotArea"] = data.PlotArea;
                    dr["Survey"] = data.Survey;
                    dr["Latitude"] = data.Latitude;
                    dr["Longitude"] = data.Longitude;
                    dr["PlotStatus"] = data.PlotStatus;
                    dr["Active"] = data.Active;
                    dr["Remarks"] = data.Remarks;
                    dr["ICSMasterId"] = data.ICSMasterId;
                    dr["FarmerRegistrationID"] = data.FarmerRegistrationID;
                    dr["FarmerRegistrationDate"] = data.FarmerRegistrationDate;
                    dr["InspectionDate"] = data.InspectionDate;
                    dr["ApprovalDate"] = data.ApprovalDate;
                    dr["RenewalPeriod"] = data.RenewalPeriod;
                    dr["FileName"] = data.FileName;
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
                var sql = @"SELECT Id, FileName FROM [MST].[FarmerMasterPlot]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

   
    }

    public class FarmerMasterPlot : BaseModel
    {

        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string FarmerMasterId { get; set; }
        public string PlotNameNo { get; set; }
        public string PlotArea { get; set; }
        public string Survey { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string PlotStatus { get; set; }
        public string Active { get; set; }
        public string Remarks { get; set; }
        public string ICSMasterId { get; set; }
        public string FarmerRegistrationID { get; set; }
        public string FarmerRegistrationDate { get; set; }
        public string InspectionDate { get; set; }
        public string ApprovalDate { get; set; }
        public string RenewalPeriod { get; set; }
        public string FileName { get; set; }

        /// <summary>

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