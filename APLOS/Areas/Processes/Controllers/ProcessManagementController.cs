using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessManagementController : Controller
    {
        string TableName = "ProcessManagement";
        private readonly ISqlRepository _sqlRepository;
        public ProcessManagementController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetProcessManagementDataList()
        {
            string sql = @"select PM.Id, PM.StandardName, PM.UserName, PM.Process, EI.SystemId ResponsiblePerson ,EI.EmployeeName EmployeeName
                        , FORMAT(PM.MinSPTTime, 'hh:mm tt')MinSPTTime ,FORMAT(PM.MaxSPTTime, 'hh:mm tt')MaxSPTTime, FORMAT(PM.StandardSPTTime, 'hh:mm tt')StandardSPTTime, PM.Remarks
                        from dbo.ProcessManagement PM
                        LEFT JOIN HKP.Process P on  P.Id = PM.Process
                        left join EmployeeInformation EI on EI.SystemId = PM.ResponsiblePerson";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadProcessParameterData()
        {
            string sql = @"select PM.Id, PM.[Sequence], PM.ItemName, PM.UOMId, PM.[Max], PM.[Min], PM.IsUtilityApplicable, PM.Remarks from dbo.ProcessParameter PM
                           left join SCS.UnitOfMeasurement UOM on UOM.Id = PM.UOMId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadEntityDetails(string headerId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            if (headerId != null) {
                sql = @"select PME.Id, PME.IsActive, PME.IsActive Flag  ,E.Id EntityId,E.EntityType,E.UserName Entity,E.Code
                            from ORG.Entity E
                            left join dbo.ProcessManagementEntity PME on PME.EntityId = E.Id
                            where E.Active = 1 order by PME.IsActive desc";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            else
            {
                sql = @"select ''Id, E.Id EntityId,E.EntityType,E.UserName Entity,E.Code
                            from ORG.Entity E
                            
                            where E.Active = 1 order by E.Id desc";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
             
        }

        
        public ActionResult LoadProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id Value, StandardName Text from HKP.Process where Active = 1 order by Text";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult LoadSubProcessList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id Value, StandardName Text from HKP.SubProcess where Active = 1 order by Text";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult LoadMaterialGrid(string headerId)
        {
            string sql = @"select distinct MM.Id, MM.Code MaterialCode ,MM.UserName Material, MC.UserName MaterialCategory, MGM.UserName MaterialGroup
, MMA.Code ArticleCode,MMA.StandardName MaterialArticle, MT.UserName MaterialType
from MST.MaterialMaster MM
                            left join MST.MaterialGroupMaster MGM on MGM.Id = MM.MaterialGroupMasterId
							left join HKP.MaterialType MT on MT.Id = MGM.MaterialTypeId
                            left join HKP.MaterialCategory MC on MC.ID = MM.MaterialCategoryId
							left join MST.MaterialMasterArticle MMA on MMA.MaterialMasterId = MM.Id
                            where MM.Active = 1 and MGM.Active = 1 and MM.MaterialCategoryId is not null";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadUtilityGrid(string headerId)
        {
            string sql = "";
            if (headerId != null)
            {
                sql = @"select PMU.Id, PMU.IsActive, PMU.IsActive Flag , PMU.[Min], PMU.[Max] , UM.Id, UM.UserName UtilityName, UM.StandardName UtilityStdName, UM.UtilityCategory, UM.UtilitySubCategory ,UOM.UserName UOM 
from UtilityMaster UM
left join [dbo].[ProcessManagementUtility] PMU on PMU.UtilityMasterId = UM.Id
left join SCS.UnitOfMeasurement UOM on UOM.Id = UM.UoMId
where UM.Active = 1 order by PMU.IsActive desc";
            }
            else
            {
                sql = @"select UM.Id, UM.UserName UtilityName, UM.StandardName UtilityStdName, UM.UtilityCategory, UM.UtilitySubCategory ,UOM.UserName UOM from UtilityMaster UM
                            left join SCS.UnitOfMeasurement UOM on UOM.Id = UM.UoMId
                            where UM.Active = 1";
            }
            
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadResponsiblePopupData()
        {
            try
            {
              
                string CmdText = @"SELECT 
                                  Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                         ,EMP.GenderID
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
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                     
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                                        Where EMP.EmployeeStatus='Active' 
                                       
                                        --ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                

                return Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public ActionResult GetUOM()
        {
            string sql = @"Select Id Value, UserName Text from SCS.UnitOfMeasurement";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        
        public ActionResult LoadWorkCenterDetails(string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select WM.Id, WM.Code ,WM.UserName Workcenter, WC.UserName WorkcenterCategory, WCS.UserName WorkcenterSubCategory, P.UserName Process, WM.Capacity
                            , UOM.UserName UOM 
                            from SCS.WorkCenterMaster WM
							--LEFT JOIN [MST].[QualityManagementWorkCenter] QMW ON QMW.WorkCenterMasterId=WM.Id
							LEFT JOIN HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
                            LEFT JOIN HKP.WorkCenterSubCategory WCS on WCS.Id = WM.WorkCenterSubcategoryId
                            left join HKP.Process P on P.Id = WM.ProcessId
                            LEFT JOIN SCS.UnitOfMeasurement UOM on UOM.Id = WM.UoMId 
                            where WM.Active = 1 and WM.ProcessId = '"+ processId + "'-- and  WM.EntityId in ('118')";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
       

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> data)
        {

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ProcessManagement where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.ProcessManagement where StandardName='" + data["StandardName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Standard Name already exists!!!");



                con.OpenDataSetThroughAdapter("select * from dbo.ProcessManagement where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult SaveProcessParameter(Dictionary<string, object> data)
        {

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ProcessParameter where ItemName='" + data["ItemName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Item Name already exists!!!");

                if (Convert.ToDecimal(data["Min"]) > Convert.ToDecimal(data["Max"]))
                {
                    throw new Exception("Max value should be greater then Min");
                }

               

                con.OpenDataSetThroughAdapter("select * from dbo.ProcessParameter where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Insert });

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

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.ProcessParameter");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public ActionResult SaveProcessEntity(List<Dictionary<string, object>> datalist, string headerid)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsChild;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessManagementEntity]  where ProcessManagementId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in datalist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["ProcessManagementId"] = headerid;
                        dr["EntityId"] = item["EntityId"];

                       dr["IsActive"] = item["Flag"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.VehiclePurposeResponsiblePerson", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["ProcessManagementId"] = headerid;
                        dr["EntityId"] = item["EntityId"];

                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = datalist,  Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult SaveProcessMaterial(List<Dictionary<string, object>> datalist, string headerid)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsChild;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessManagementMaterial]  where ProcessManagementId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in datalist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["ProcessManagementId"] = headerid;
                        dr["MaterialMasterId"] = item["Id"];

                        dr["IsActive"] = item["Flag"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.VehiclePurposeResponsiblePerson", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["ProcessManagementId"] = headerid;
                        dr["MaterialMasterId"] = item["Id"];

                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = datalist, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult SaveProcessUtility(List<Dictionary<string, object>> datalist, string headerid)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsChild;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessManagementUtility]  where ProcessManagementId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in datalist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["ProcessManagementId"] = headerid;
                        dr["UtilityMasterId"] = item["Id"];
                        dr["Min"] = item["Min"];
                        dr["Max"] = item["Max"];
                        dr["IsActive"] = item["isSelected"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.VehiclePurposeResponsiblePerson", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["ProcessManagementId"] = headerid;
                        dr["UtilityMasterId"] = item["Id"];
                        dr["Min"] = item["Min"];
                        dr["Max"] = item["Max"];
                        //dr["Remarks"] = item["Remarks"];
                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = datalist, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult SaveProcessWorkcenter(List<Dictionary<string, object>> datalist, string headerid)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsChild;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessManagementWorkcenter]  where ProcessManagementId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in datalist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["ProcessManagementId"] = headerid;
                        dr["WorkcenterId"] = item["WorkcenterId"];

                        dr["IsActive"] = item["Flag"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.VehiclePurposeResponsiblePerson", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["ProcessManagementId"] = headerid;
                        dr["WorkcenterId"] = item["WorkcenterId"];

                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = datalist, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #region OWM
        public ActionResult GetOWMData()
        {
            string sql = "select * from HKP.ProcessManagementOWM";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public double GetSequenceOWM()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM hkp.ProcessManagementOWM");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public ActionResult OWMSave(Dictionary<string,object>data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from HKP.ProcessManagementOWM where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from HKP.ProcessManagementOWM where StandardName='" + data["StandardName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Standard Name already exists!!!");



                con.OpenDataSetThroughAdapter("select * from HKP.ProcessManagementOWM where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }


        #endregion OWM

        #region GPL
        public ActionResult GetGPLData()
        {
            string sql = @"select * from hkp.ProcessManagementGPL";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public double GetSequenceGPL()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM hkp.ProcessManagementGPL");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
         }

        public ActionResult GPLSave(Dictionary<string, object> data) 
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from HKP.ProcessManagementGPL where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from HKP.ProcessManagementGPL where StandardName='" + data["StandardName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Standard Name already exists!!!");



                con.OpenDataSetThroughAdapter("select * from HKP.ProcessManagementGPL where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult DeleteProcessManagementOWM(string id)
        {
            DeleteProcessManagementGPLData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProcessManagementOWMData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = "DELETE FROM [HKP].[ProcessManagementOWM] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }

        [HttpPost]
        public ActionResult DeleteProcessManagementGPL(string id)
        {
            DeleteProcessManagementGPLData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteProcessManagementGPLData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strSQL = "DELETE FROM [HKP].[ProcessManagementGPL] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function
        #endregion GPL
    }
}