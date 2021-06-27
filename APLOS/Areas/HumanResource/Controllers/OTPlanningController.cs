using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Security.Core;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class OTPlanningController : BaseController
    {
        #region Constructor
        string TableName = "dbo.OTPlanningMaster";
        string DTableName = "dbo.OTPlanningDetail";
        private readonly ISqlRepository _sqlRepository;

        public OTPlanningController(
              ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages


        #region Operation
        [HttpGet, Authorize]
        public ActionResult GetEmpData(string Date, string ShiftId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select en.Id EntityId,en.UserName Entity
                            ,d.Id DepartmentId,d.UserName Department
                            ,s.Id SectionId,s.UserName Section
                            ,ss.Id SubSectionId,ss.UserName SubSection                             
                            ,isnull(e.NoOfEmp,0)NoOfEmp
							,m.Id ManpowerBudgetId,m.Code ManpowerBudgetCode
							,otd.Id,otd.OtPlanMst,otd.ManpowerBudgetId
							,otd.EntityId,otd.DepartmentId
							,otd.SectionId,otd.SubSectionId,otd.AllotedMan
							,otd.AllotedHour,otm.[Date],otm.PlantId,otm.ShiftId

                            from MST.ManpowerBudget m 
                            left join org.entity en on en.Id = m.EntityId 
                            left join org.Position p on p.Id=m.PositionId
                            left join ORG.Department d on d.Id=p.DepartmentId
                            left join ORG.Section s on s.Id=p.SectionId
                            left join ORG.SubSection ss on ss.Id= p.SubSectionId
						
							left join OTPlanningMaster otm on  otm.Date = '" + Date + @"' and otm.ShiftId = '" + ShiftId + @"'
					        left join OTPlanningDetail otd on otd.OtPlanMst = otm.Id
					        and otd.ManpowerBudgetId = m.Id and otd.EntityId = en.Id and otd.DepartmentId = d.Id
							and otd.SectionId = s.Id and otd.SubSectionId = ss.Id
                            left join
							
							(select COUNT (SystemId)NoOfEmp,BudgetCode from  EmployeeInformation
							group by BudgetCode							
							) e on e.BudgetCode = m.Id

                            where en.PlantId = '" + identity.PlantId + "' ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult Save(Dictionary<string, object> MasterData, List<Dictionary<string, object>> Details)
        {
            try
            {
                #region Master
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + MasterData["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string _DId = "";
                string _MId = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    MasterData["Id"] = "OTPM-" + _Id;
                    _MId = "OTPM-" + _Id;
                    MasterData["PlantId"] = identity.PlantId;
                    AddNewRow(dsMaster.Tables[0], MasterData);
                }
                else
                {
                    _MId = MasterData["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], MasterData);
                }
                #endregion data update

                #endregion

                #region Child 

                DataSet dsChild;
                
                con.OpenDataSetThroughAdapter("select * from " + DTableName + " where  OtPlanMst='" + _MId + "'", out dsChild, false, "1");
                #region data update
                foreach (var item in Details)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(DTableName, out _DId);

                        item["Id"] = "OTPD-" + _DId;
                        item["OtPlanMst"] = _MId;
                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                        //EditRow(dsChild.Tables[0].Rows[0], item);
                    }
                }
                #endregion

                #endregion
                // GetMasterData(out dsMaster);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);



                return Json(new { Error = false, Data = Details, Message = AplosMessage.Updated });

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
        public void GetMasterData(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from " + DTableName + " ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        #endregion
    }
}

//public class OTPlanningDetail : BaseModel
//{
//    #region Scalar Properties            
//    public string Id { get; set; }
//    public string OtPlanMst { get; set; }
//    public string EntityId { get; set; }    
//    public string DepartmentId { get; set; }    
//    public string SectionId { get; set; }    
//    public string SubSectionId { get; set; }    
//    public string AllotedMan { get; set; }    
//    public string AllotedHour { get; set; }    

//    #endregion Scalar Properties

//    #region Audit Properties
//    [NeverUpdate]
//    public string AddedBy { get; set; }
//    public string AddedFromIP { get; set; }
//    [NeverUpdate]
//    public DateTime AddedDate { get; set; }

//    public string UpdatedBy { get; set; }
//    public string UpdatedFromIP { get; set; }
//    public DateTime UpdatedDate { get; set; }

//    #endregion Audit Properties
//}