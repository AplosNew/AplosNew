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
    public class QMSRejectionController : BaseController
    {
        string TableName = "TRN.QMSRejection";
        string TableName1 = "TRN.QMSRejectionChild";


        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public QMSRejectionController(ISqlRepository R)
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
        public JsonResult GetProductionReference()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,Id AS Text FROM [TRN].[ProductionOrder]"), JsonRequestBehavior.AllowGet);
        }

        

        [Authorize, HttpGet]
        public JsonResult GetLocationList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MST].[QMSMaster]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetShiftMaster()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT SystemID as Value,UserName AS Text FROM [dbo].[ShiftDefination]"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetSKUList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [HKP].[StockKeepingUnit]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetDefectMasterList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [MST].[QMSDefectMaster]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public JsonResult GetGradeList()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM [HKP].[GradeMaster]"), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from TRN.QMSRejection where Id = '" + Id + "' ");


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
           
            string sql= @"select top 100 * from (select distinct qmsr.*,PO.Id as POId,Xp.UserName as Customer,p.UserName as Process,sd.UserName as Shift,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName,EmpI.EmployeeStatus as EmpIStatus,
                                                L.UserName as Location
                                                from TRN.QMSRejection qmsr inner join trn.ProductionOrder PO on qmsr.ProductionReferenceId=PO.Id
		                                        Left JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
	                                        	 Left join trn.SalesOrder SO ON SO.Id=POD.SalesOrderId
                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=SO.MasterOrderItemId
                                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                                                left join HKP.Process p on qmsr.ProcessId=p.Id      	 
                                                left join dbo.EmployeeInformation EI on qmsr.ResponsiblePersonId=EI.SystemId
                                                left join dbo.EmployeeInformation EmpI on qmsr.EmployeeId=EmpI.SystemId
	                                        	 left join MST.QMSMaster L on qmsr.LocationId=L.Id
												 left join dbo.ShiftDefination sd on qmsr.ShiftMasterId=sd.SystemID)AS TEMP WHERE " + strkey + " ";




          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ShiftMasterId='" + data["ShiftMasterId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Shift already exists!!!");

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


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0  && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "QR" + _Id;
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
            string sql = @"select * from [TRN].[QMSRejection] where CostingGroupId = '" + id + "'";


            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where QMSRejectionMasterId= '" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Child data");
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
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from TRN.QMSRejection where Id='" + Id + @"')
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
                   AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId,'') from TRN.QMSRejection where Id='" + Id + @"')
                  order by EmployeeCodePreFix,EmployeeCodeNumeric";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost]
        public JsonResult CreateInspectionChild(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where StockKeepingUnitId='" + data["StockKeepingUnitId"] + "' AND  Id<>'" + data["Id"] + "' and QMSRejectionMasterId='" + data["QMSRejectionMasterId"] + "'  ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Stock Keeping Unit already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where GradeMasterId='" + data["GradeMasterId"] + "' AND  Id<>'" + data["Id"] + "' and QMSRejectionMasterId='" + data["QMSRejectionMasterId"] + "'  ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Grade Master already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0 && data["Id"] == null)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "RC" + _Id;
                    AddNewRowRejectionChild(dsMaster.Tables[0], data);
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

        private void AddNewRowRejectionChild(DataTable dt, Dictionary<string, object> sourceData)
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
        public JsonResult GetListRejectionChild(string QMSRejectionId)
        {

            //     var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"select top 100 * from (select qmsrc.*,qmsdm.UserName as DefectMaster,sku.UserName as SKU, g.UserName as Grade             
                                 from TRN.QMSRejectionChild qmsrc left join MST.QMSDefectMaster qmsdm on qmsrc.QMSDefectMasterId=qmsdm.Id
                                 left join HKP.StockKeepingUnit sku on qmsrc.StockKeepingUnitId=sku.Id
                                 left join HKP.GradeMaster g on qmsrc.GradeMasterId=g.Id
                                 where QMSRejectionMasterId= '" + QMSRejectionId + "') AS TEMP order by DefectMaster";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult DeleteRejectionChild(string Id)
        {
            try
            {
                string sql = @" delete from TRN.QMSRejectionChild where Id='" + Id + "'";

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Rejection Child deleted successfully"
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