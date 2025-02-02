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


#endregion Using

namespace Aplos.Areas.Farming.Controllers
{
    public class DailyCropRateController : BaseController
    {
        string TableName = "MST.DailyCroprate";
        


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public DailyCropRateController(ISqlRepository R)
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
        public JsonResult GetLocation()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [HKP].[CropRateLocation]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCrop()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName AS Text FROM [MST].[CropMaster]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCropType(string CropId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct ct.Id as Value,ct.UserName as Text from HKP.CropType ct inner join MST.CropTypeMaster ctt on ct.Id=ctt.CropTypeId inner join MST.CropMaster cm on ctt.CropMasterId = '" + CropId + "' "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetPlanMaximumRate( string CropTypeId)
        {
            return Json(_sqlRepository.GetDataCollection("select distinct ct.Id as Value, ct.MaximumRate as Text from MST.CropTypeMaster ct where CropTypeId='" + CropTypeId + "' "), JsonRequestBehavior.AllowGet);
        }




        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from MST.DailyCroprate where Id = '" + Id + "' ");


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
           

            string sql = @"select top 100 * from (select distinct dcr.*,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,crl.UserName as Location ,cm.UserName as Crop,ct.UserName as CropType, ctm.Id as PlanMaximumRateId, ctm.MaximumRate as PlanMaximumRate
                                                 from MST.DailyCroprate dcr left join dbo.EmployeeInformation EI on dcr.ApproveById=EI.SystemId 
												 left join HKP.CropRateLocation crl on crl.Id=dcr.LocationId 
												 left join MST.CropMaster cm on cm.Id=dcr.CropId
												 left join HKP.CropType ct on ct.id=dcr.CropTypeId
												 left join MST.CropTypeMaster ctm on ctm.CropMasterId=dcr.CropId and ctm.CropTypeId=dcr.CropTypeId
												 ) AS TEMP WHERE 1=1 order by Date desc ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "DailyCroprate", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where   Date='" + data["Date"] + "'AND LocationId='" + data["LocationId"] + "'AND CropId='" + data["CropId"] + "' AND CropTypeId='" + data["CropTypeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                {
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Date, Location, Crop and CropType already exists!!!");
                }
                
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "DR" + GetPK();
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


            try
            {
             
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
                   AND isnull(Emp.SystemID,'') not in (select isnull(ApproveById,'') from MST.DailyCroprate where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }    
}