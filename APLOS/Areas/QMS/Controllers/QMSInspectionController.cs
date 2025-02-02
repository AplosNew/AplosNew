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

namespace Aplos.Areas.QMS.Controllers
{
    public class QMSInspectionController : BaseController
    {
        string TableName = "TRN.QMSInspection";
        string TableName1 = "TRN.QMSInspectionChild";


        //authentication for
        //GetList Create Delete


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public QMSInspectionController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       


        [Authorize]
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
        public JsonResult GetProcess()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [HKP].[Process]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetInspectionLevel(string InspectionMasterId)
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,Category AS Text FROM [hkp].[InspectionMaster] where Id='" + InspectionMasterId + "' "), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetInspectionMasterList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[InspectionMaster]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetProductionReference()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,Id AS Text FROM [TRN].[ProductionOrder]"), JsonRequestBehavior.AllowGet);
        }

        

        [Authorize, HttpGet]
        public JsonResult GetInspectionType()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[InspectionType]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetLocationList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MST].[QMSMaster]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetStatusList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[QualityStatus]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getdefectmasterlist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MST].[QMSDefectMaster]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getdefectzonelist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[DefectZone]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getskilllist()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [hkp].[Skill]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from TRN.QMSInspection where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
            string sql= @"select top 100 * from (select distinct qmsi.*,PO.Id as POId,Xp.UserName as Customer,p.UserName as Process,it.UserName as InspectionType,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName,EmpI.EmployeeStatus as EmpIStatus,
                                                ipm.UserName as InspectionMaster,ipm.Category as InspectionLevel,L.UserName as Location
                                                from TRN.QMSInspection qmsi inner join trn.ProductionOrder PO on qmsi.ProductionReferenceId=PO.Id
		                                        Left JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
	                                        	 Left join trn.SalesOrder SO ON SO.Id=POD.SalesOrderId
                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=SO.MasterOrderItemId
                                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                                                left join HKP.Process p on qmsi.ProcessId=p.Id
	                                        	 left join HKP.InspectionType it on qmsi.InspectionTypeId=it.Id
                                                left join dbo.EmployeeInformation EI on qmsi.ResponsiblePersonId=EI.SystemId
                                                left join dbo.EmployeeInformation EmpI on qmsi.EmployeeId=EmpI.SystemId
	                                        	 left join HKP.InspectionMaster ipm on qmsi.InspectionMasterId=ipm.Id
	                                        	 left join MST.QMSMaster L on qmsi.LocationId=L.Id
												 left join HKP.InspectionMaster on qmsi.InspectionLevelId=ipm.Id)AS TEMP WHERE " + strkey + " ";




          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, Dictionary<string, object> InspectionChildData)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where InspectionMasterId='" + data["InspectionMasterId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Inspection Master already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where InspectionTypeId='" + data["InspectionTypeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Inspection Type already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where InspectionLevelId='" + data["InspectionLevelId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Inspection Level already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmployeeId='" + data["EmployeeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Employee already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ProcessId='" + data["ProcessId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Process already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where LocationId='" + data["LocationId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Location already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ResponsiblePersonId='" + data["ResponsiblePersonId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Responsible Person already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ProductionReferenceId='" + data["ProductionReferenceId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Production Reference already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where StatusId='" + data["StatusId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Status already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "QI" + _Id;
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
                string InspectionMasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                CreateInspectionChild(InspectionChildData,InspectionMasterId);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            string sql = @"select * from [TRN].[QMSInspection] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where QMSInspectionId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Defect Master");
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
        public ActionResult LoadAllResPersonDetailsForSelection(string Id)
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
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from TRN.QMSInspection where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        // Employee field
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
                   AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId,'') from TRN.QMSInspection where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        // Employee field
        [HttpPost, Authorize]
        public ActionResult LoadAllDefResPonDetailsForSelection(string Id)
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
                   AND isnull(Emp.SystemID,'') not in (select isnull(DefectResponsiblePersonId,'') from TRN.QMSInspectionChild where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost]
        public JsonResult CreateInspectionChild(Dictionary<string, object> data, string InspectionMasterId)
        {
            try
            {
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where QMSDefectMasterId='" + data["QMSDefectMasterId"] + "' AND  Id<>'" + data["Id"] + "' and QMSInspectionId='" + InspectionMasterId + "'  ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Defect Master already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where QMSDefectZoneId='" + data["QMSDefectZoneId"] + "' AND  Id<>'" + data["Id"] + "' and QMSInspectionId='" + InspectionMasterId + "'  ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Defect Zone already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where SkillId='" + data["SkillId"] + "' AND  Id<>'" + data["Id"] + "' and QMSInspectionId='" + InspectionMasterId + "'  ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Skill already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where DefectResponsiblePersonId='" + data["DefectResponsiblePersonId"] + "' AND  Id<>'" + data["Id"] + "' and QMSInspectionId='" + InspectionMasterId + "'  ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Defect Responsible Person already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "IC" + _Id;
                    data["QMSInspectionId"] = InspectionMasterId;
                    AddNewRowInspectionChild(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    //  EditRow(dsMaster.Tables[0].Rows[0], data);
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

        private void AddNewRowInspectionChild(DataTable dt, Dictionary<string, object> sourceData)
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

        [HttpGet]
        public JsonResult GetListInspectionChild(string QMSInspectionId)
        {

       //     var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"select top 100 * from (select qmsic.*,qmsdm.UserName as DefectMaster,dz.UserName as DefectZone, s.UserName as Skill,einfo.EmployeeName as DefResPonName,
                                 einfo.EmployeeCode as DefResPonCode,einfo.EmployeeStatus as EmpICStatus
                                 from TRN.QMSInspectionChild qmsic left join MST.QMSDefectMaster qmsdm on qmsic.QMSDefectMasterId=qmsdm.Id
                                 left join HKP.DefectZone dz on qmsic.QMSDefectZoneId=dz.Id
                                 left join HKP.Skill s on qmsic.SkillId=s.Id
								 left join dbo.EmployeeInformation einfo on qmsic.DefectResponsiblePersonId=einfo.SystemId
                                 where QMSInspectionId= '" + QMSInspectionId + "') AS TEMP order by DefectMaster";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteInspectionChild(string Id)
        {
            try
            {
                string sql = @" delete from TRN.QMSInspectionChild where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Inspection Child deleted successfully"
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

    }

}