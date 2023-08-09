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

        public ActionResult GetDProcessManagementDataList()
        {
            string sql = @"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadEntityDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select E.Id EntityId,E.EntityType,E.UserName Entity,E.Code
                            from ORG.Entity E
                            where E.Active = 1 order by E.Id desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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

        
        public ActionResult LoadMaterialGrid()
        {
            string sql = @"select distinct MM.Id, MM.Code MaterialCode ,MM.UserName Material, MC.UserName MaterialCategory, MGM.UserName MaterialGroup
, MMA.Code ArticleCode,MMA.StandardName MaterialArticle
from MST.MaterialMaster MM
                            left join MST.MaterialGroupMaster MGM on MGM.Id = MM.MaterialGroupMasterId
                            left join HKP.MaterialCategory MC on MC.ID = MM.MaterialCategoryId
							left join MST.MaterialMasterArticle MMA on MMA.MaterialMasterId = MM.Id
                            where MM.Active = 1 and MGM.Active = 1 and MM.MaterialCategoryId is not null";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadUtilityGrid()
        {
            string sql = @"select UM.Id, UM.UserName UtilityName, UM.StandardName UtilityStdName, UOM.UserName UOM from UtilityMaster UM
                            left join SCS.UnitOfMeasurement UOM on UOM.Id = UM.UoMId
                            where UM.Active = 1";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadResponsiblePopupData()
        {
            try
            {
              
                string CmdText = @"SELECT 
                                  Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                         ,EMP.GenderID
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
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

        
        public ActionResult LoadWorkCenterDetails()
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
                            where WM.Active = 1 ";
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
    }
}