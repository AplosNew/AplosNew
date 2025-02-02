using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class BonusRetainedDisbursementController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public BonusRetainedDisbursementController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetBonusRetainedData(string DisbursementDate)
        {
            string DisbursementDate2 = Convert.ToDateTime(DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 0 CheckBoxSelect, spc.EmpInfoSystemID ,'' SystemID  
                            --,spm.YearNo
                            --,spm.MonthNo
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount
                            --,sh.SalaryHead
                            --,spc.SystemID
   
                              ,EI.EmployeeCode
                              ,EI.EmployeeName
                              ,format(EI.DOJ,'dd-MMM-yyyy') DOJ                            
                              ,DG.UserName GivenDesignation
                              ,DP.UserName Department
                              ,PMB.Code
                              ,PR.UserName PositionName
                              ,E.UserName EntityName
                              ,DSG.UserName Designation
                              ,PR.DesignationId
                              ,PG.StandardName PayRollGroupName
                              ,PG.Id PayRollGroupId							
                              ,ld.UserName LegalDesignation
                              ,ec.UserName EmployeeCategory
                              ,LSG.UserName SalaryGrade ,s.UserName  Section,sb.UserName SubSection 
                             FROM  SalaryProcChild spc
                             LEFT JOIN SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
 
                             LEFT JOIN dbo.Employeeinformation EI on ei.SystemId=spc.EmpInfoSystemID                        
                             LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                             LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                             LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                             LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                             LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
                             LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=ei.LegalDesignationId---- 
                             LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                             Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                             Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId 
                             LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                             LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                             LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId----- 
                             LEFT join [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = EI.LegalDesignationId and lsgd.PlantId=ei.PlantId
                             LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id 
                             LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId                              
                             LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId  
							 LEFT JOIN ORG.Section s on s.id=PR.SectionId
                             LEFT JOIN ORG.SubSection sb on sb.id=PR.SubSectionId
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                             WHERE   sl.IsLocked=1 and sh.HeadCategory IN ('Other Bonus','RetainedBonus','Monthly Bonus Retain')  AND spc.DisbusmentAmount>0 and IsRetained=1
                            and (spm.YearNo<=year('" + DisbursementDate2 + @"') or (spm.YearNo<=year('" + DisbursementDate + @"') and spm.MonthNo<=month('" + DisbursementDate + @"')))
                            and spc.PlantID='" + identity.PlantId + @"'
	                        and ISNULL( sd.Id,'')=''
 
                            group by spc.EmpInfoSystemID, EI.SystemId
                              ,EI.EmployeeCode
                              ,EI.EmployeeName
                              ,EI.DOJ              
                              ,DG.UserName
                              ,DP.UserName 
                              ,PMB.Code
                              ,PR.UserName 
                              ,E.UserName 
                              ,DSG.UserName
                              ,PR.DesignationId
                              ,PG.StandardName 
                              ,PG.Id 							
                              ,ld.UserName 
                              ,ec.UserName 
                              ,LSG.UserName 
                              ,s.UserName  ,sb.UserName 
                             order by spc.EmpInfoSystemID";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSevedBonusRetainedData(string BonusRetainedDisbursementMasterId)
        {
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 0 CheckBoxSelect,BRDD.EmpSystemId EmpInfoSystemID,BRDD.Id SystemID                            
                              ,BRDD.Amount DisbusmentAmount                          
                              ,EI.EmployeeCode
                              ,EI.EmployeeName
                              ,format(EI.DOJ,'dd-MMM-yyyy') DOJ                            
                              ,DG.UserName GivenDesignation
                              ,DP.UserName Department
                              ,PMB.Code
                              ,PR.UserName PositionName
                              ,E.UserName EntityName
                              ,DSG.UserName Designation
                              ,PR.DesignationId
                              ,PG.StandardName PayRollGroupName
                              ,PG.Id PayRollGroupId							
                              ,ld.UserName LegalDesignation
                              ,ec.UserName EmployeeCategory
                              ,LSG.UserName SalaryGrade ,s.UserName  Section,sb.UserName SubSection 
                             FROM  BonusRetainedDisbursementDetail BRDD
							 LEFT JOIN BonusRetainedDisbursementMaster BRDM on BRDM.Id=BRDD.BonusRetainedDisbursementMasterId 
                             LEFT JOIN dbo.Employeeinformation EI on ei.SystemId=BRDD.EmpSystemId                        
                             LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                             LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                             LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                             LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                             LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
                             LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=ei.LegalDesignationId---- 
                             LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                             Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
                             Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId 
                             LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                             LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                             LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId----- 
                             LEFT join [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = EI.LegalDesignationId and lsgd.PlantId=ei.PlantId
                             LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id 
                             LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId                              
                             LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId  
							 LEFT JOIN ORG.Section s on s.id=PR.SectionId
                             LEFT JOIN ORG.SubSection sb on sb.id=PR.SubSectionId                          
                             WHERE BRDD.PlantID='" + identity.PlantId + @"' and BRDD.BonusRetainedDisbursementMasterId='" + BonusRetainedDisbursementMasterId + @"' 
                             order by BRDD.EmpSystemId";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }




        [HttpGet, Authorize]
        public ActionResult GetBonusRetainedDetails(string EmployeeSystemId, string DisbursementDate)
        {
            string DisbursementDate2 = Convert.ToDateTime(DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount,sh.SalaryHead,spc.SystemID from SalaryProcChild spc
                            left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                            left join SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                            left join SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
  LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                             WHERE   sl.IsLocked=1 and sh.HeadCategory IN ('Other Bonus','RetainedBonus','Monthly Bonus Retain') AND spc.DisbusmentAmount>0 and IsRetained=1
                            and (spm.YearNo<=year('" + DisbursementDate2 + @"') or (spm.YearNo<=year('" + DisbursementDate + @"') and spm.MonthNo<=month('" + DisbursementDate + @"')))
                            and spc.PlantID='" + identity.PlantId + @"' AND spc.EmpInfoSystemID='" + EmployeeSystemId + @"' and ISNULL( sd.Id,'')=''
                           order by spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetBonusRetainedSavedDetails(string EmployeeSystemId, string DisbursementDate)
        {
            string DisbursementDate2 = Convert.ToDateTime(DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount,sh.SalaryHead,spc.SystemID from SalaryProcChild spc
                            left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                            left join SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                            left join SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
  LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                            WHERE   sl.IsLocked=1 and sh.HeadCategory IN ('Other Bonus','Ex-Gratia','Statutory Bonus') AND spc.DisbusmentAmount>0
                            and (spm.YearNo<=year('" + DisbursementDate2 + @"') or (spm.YearNo<=year('" + DisbursementDate + @"') and spm.MonthNo<=month('" + DisbursementDate + @"')))
                            and spc.PlantID='" + identity.PlantId + @"' AND spc.EmpInfoSystemID='" + EmployeeSystemId + @"' 
                           order by spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult GetBonusRetainedDisbursementMasterData()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" Select Id,Format(DisbursementDate,'dd-MMM-yyyy') DisbursementDate,Format(PaymentDate,'dd-MMM-yyyy') PaymentDate ,Description from dbo.BonusRetainedDisbursementMaster 
                            where Plantid='" + identity.PlantId + @"'
                            order by Convert(datetime, DisbursementDate) desc ";

            var data = _sqlRepository.GetDataCollection(sql);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveBonusRetainedData(CustomPara CustomPara, List<BonusRetainedModel> BonusRetainedList)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {

                string MasterID = string.Empty;
                DataSet dsMaster;
                DataSet dsBonusRetainedDisbursementDetail;
                DataSet dsBonusRetainedDataWithDetails;
                DataSet dsSalaryDisbursementInAcc;
                bplib.clsGenID objGenID = new bplib.clsGenID();

                string sqls = "select *from BonusRetainedDisbursementMaster where PlantId='" + identity.PlantId + @"' and DisbursementDate='" + CustomPara.DisbursementDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqls, out dsMaster, false, "1");
                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = " PlantId='" + identity.PlantId + @"' and DisbursementDate='" + CustomPara.DisbursementDate + @"'";

                if (dvMaster.Count == 0)
                {
                    string sIDM = string.Empty;
                    
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BonusRetainedDisbursementMaster", out sIDM);
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    MasterID = "BRM" + sIDM;
                    dr["Id"] = MasterID;
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["CompanyGroupId"] = identity.CompanyGroupId.ToString();
                    dr["DisbursementDate"] = CustomPara.DisbursementDate.ToString();
                    dr["Description"] = CustomPara.Description.ToString();

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);



                }
                else
                {

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();
                    MasterID = dr["Id"].ToString();

                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["CompanyGroupId"] = identity.CompanyGroupId.ToString();
                    dr["DisbursementDate"] = CustomPara.DisbursementDate.ToString();
                    dr["Description"] = CustomPara.Description.ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                dvMaster.RowFilter = null;






                string DisbursementDate2 = Convert.ToDateTime(CustomPara.DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");
                GetBonusRetainedDataWithDetails(CustomPara.DisbursementDate, out  dsBonusRetainedDataWithDetails);
                string sqlSalaryDisbursementInAcc = @"select * from SalaryDisbursementInAcc where PlantId='' 
                                                     and  (YearNo<=year('" + DisbursementDate2 + @"') or (YearNo<=year('" + CustomPara.DisbursementDate + @"') and MonthNo<=month('" + CustomPara.DisbursementDate + @"')))";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlSalaryDisbursementInAcc, out dsSalaryDisbursementInAcc, false, "1");







                string sql = @"select *from BonusRetainedDisbursementDetail where BonusRetainedDisbursementMasterId='" + MasterID + "@' and PlantId='" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBonusRetainedDisbursementDetail, false, "1");




                string sID = string.Empty;
                //bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BonusRetainedDisbursementDetail", out sID);


                string BonusRetainedDisbursementDetailPK= "BRMD" + sID;
                int pk = 0;

                if (BonusRetainedList.Count > 0)
                {
                    DataView dvBonusRetainedDisbursementDetail = new DataView(dsBonusRetainedDisbursementDetail.Tables[0]);
                    foreach (var item in BonusRetainedList)
                    //for (int i = 0; i < DailyAllowanceData.Count(); i++)
                    {

                        string BonusRetainedDisbursementDetailPK2 = string.Empty;
                        dvBonusRetainedDisbursementDetail.RowFilter = "EmpSystemId='" + item.EmpInfoSystemID.ToString() + "'";

                        if (dvBonusRetainedDisbursementDetail.Count == 0)
                        {
                            BonusRetainedDisbursementDetailPK2 = BonusRetainedDisbursementDetailPK +"_"+ pk.ToString();
                             DataRow dr = dsBonusRetainedDisbursementDetail.Tables[0].NewRow();
                            dr["Id"] = BonusRetainedDisbursementDetailPK2;
                            dr["PlantID"] = identity.PlantId.ToString();                           
                            dr["BonusRetainedDisbursementMasterId"] = MasterID.ToString();
                            dr["EmpSystemId"] = item.EmpInfoSystemID;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["IsDisbursed"] = true;
                            dr["IsApproved"] = true;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            //dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;
                            dsBonusRetainedDisbursementDetail.Tables[0].Rows.Add(dr);

                        }
                        else
                        {

                            DataRow dr = dvBonusRetainedDisbursementDetail[0].Row;
                            dr.BeginEdit();
                            BonusRetainedDisbursementDetailPK2 = dr["Id"].ToString();


                            dr["PlantID"] = identity.PlantId.ToString();
                            dr["PlantID"] = identity.PlantId.ToString();
                            dr["BonusRetainedDisbursementMasterId"] = MasterID.ToString();
                            dr["EmpSystemId"] = item.EmpInfoSystemID;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["IsDisbursed"] = true;
                            dr["IsApproved"] = true;
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }
                        dvBonusRetainedDisbursementDetail.RowFilter = null;



                        DataView dvSalaryDisbursementInAcc = new DataView(dsSalaryDisbursementInAcc.Tables[0]);
                        DataView dvBonusRetainedDataWithDetails = new DataView(dsBonusRetainedDataWithDetails.Tables[0]);
                        dvBonusRetainedDataWithDetails.RowFilter = "EmpInfoSystemId='" + item.EmpInfoSystemID.ToString() + "'";
                        if (dvBonusRetainedDataWithDetails.Count>0)
                        {
                           


                            for (int i = 0; i < dvBonusRetainedDataWithDetails.Count; i++)
                            {
                                dvSalaryDisbursementInAcc.RowFilter = "EmpSystemId='" + item.EmpInfoSystemID.ToString() + "' AND SalaryHeadId='"+ dvBonusRetainedDataWithDetails[i]["SalaryHeadId"].ToString() + @"' AND MonthNo='" + dvBonusRetainedDataWithDetails[i]["MonthNo"].ToString() + @"' AND YearNo='" + dvBonusRetainedDataWithDetails[i]["YearNo"].ToString() + @"'";

                                if (dvSalaryDisbursementInAcc.Count == 0)
                                {
                                    string sID3 = string.Empty;                                   
                                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalaryDisbursementInAcc", out sID3);                                
                                    DataRow dr = dsSalaryDisbursementInAcc.Tables[0].NewRow();
                                    dr["Id"] = "SDIA" + sID3; ;
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["BonusRetainedDisbursementDetailId"] = BonusRetainedDisbursementDetailPK2.ToString();
                                    dr["EmpSystemId"] = item.EmpInfoSystemID;
                                    dr["Amount"] = dvBonusRetainedDataWithDetails[i]["DisbusmentAmount"].ToString();
                                    dr["SalaryHeadId"] = dvBonusRetainedDataWithDetails[i]["SalaryHeadId"].ToString();
                                    dr["MonthNo"] = dvBonusRetainedDataWithDetails[i]["MonthNo"].ToString();
                                    dr["YearNo"] = dvBonusRetainedDataWithDetails[i]["YearNo"].ToString();



                                    dr["AddedBy"] = identity.Name;
                                    dr["DateAdded"] = System.DateTime.Now.ToString();
                                  
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                                    dsSalaryDisbursementInAcc.Tables[0].Rows.Add(dr);

                                }
                                else
                                {

                                    DataRow dr = dvSalaryDisbursementInAcc[0].Row;
                                    dr.BeginEdit();
                                    BonusRetainedDisbursementDetailPK2 = dr["Id"].ToString();                                  
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["BonusRetainedDisbursementDetailId"] = BonusRetainedDisbursementDetailPK2.ToString();
                                    dr["EmpSystemId"] = item.EmpInfoSystemID;
                                    dr["Amount"] = dvBonusRetainedDataWithDetails[i]["DisbusmentAmount"].ToString();
                                    dr["SalaryHeadId"] = dvBonusRetainedDataWithDetails[i]["SalaryHeadId"].ToString();
                                    dr["MonthNo"] = dvBonusRetainedDataWithDetails[i]["MonthNo"].ToString();
                                    dr["YearNo"] = dvBonusRetainedDataWithDetails[i]["YearNo"].ToString();

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                                  
                                    dr.EndEdit();
                                }
                                dvSalaryDisbursementInAcc.RowFilter = null;
                            }

                        }
                        dvBonusRetainedDataWithDetails.RowFilter = null;



                        pk++;
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsBonusRetainedDisbursementDetail,dsSalaryDisbursementInAcc);
            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Success });
        }




        public void GetBonusRetainedDataWithDetails(string DisbursementDate,out DataSet dsBonusRetainedDataWithDetails)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {

        

                string DisbursementDate2 = Convert.ToDateTime(DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");
               
                string sql = @"select spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount,sh.SalaryHead,spc.SalaryHeadID from SalaryProcChild spc
                            left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                            left join SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                            left join SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                            left join SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                            WHERE   sl.IsLocked=1 and sh.HeadCategory IN ('Other Bonus','Ex-Gratia','Statutory Bonus') AND spc.DisbusmentAmount>0
                            and (spm.YearNo<=year('" + DisbursementDate2 + @"') or (spm.YearNo<=year('" + DisbursementDate + @"') and spm.MonthNo<=month('" + DisbursementDate + @"')))
                            and spc.PlantID='" + identity.PlantId + @"' and ISNULL( sd.Id,'')=''

                           order by spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount";
               
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBonusRetainedDataWithDetails, false, "1");
              
               
            }
            catch (Exception ex)
            {

                throw (ex);
            }

           
        }





        [HttpPost]
        public ActionResult DeleteBonusRetainedDisbursement(string SystemID,string EmpInfoSystemID)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("delete from SalaryDisbursementInAcc where BonusRetainedDisbursementDetailId='" + SystemID + @"'", true, "1");
                objCon.ExecuteNonQueryWrapper("delete from BonusRetainedDisbursementDetail where id='" + SystemID + @"'", true, "1");
               
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)

            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                }
                catch (Exception exx)
                {

                    throw (ex);

                }


            }
            finally
            {

                objCon.CloseConnection();
                objCon = null;
            }

            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion -- Operations  
    }
    public class BonusRetainedModel
    {
        public string EmpInfoSystemID { get; set; }
        public string DisbusmentAmount { get; set; }

    }
    public class CustomPara
    {
        public string DisbursementDate { get; set; }
        public string Description { get; set; }

    }
}