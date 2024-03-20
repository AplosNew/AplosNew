using Aplos.Controllers;
using Aplos.Properties;
using bplib;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;


namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryLockController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public SalaryLockController(
            ISqlRepository sqlRepository
            )
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
        [HttpPost]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            JsonResult json = Json(_sqlRepository.GetDataCollection(new clsSalaryLock().GetEmpInfo((CustomIdentity)Thread.CurrentPrincipal.Identity, effectiveDate, salaryProcessId, isActive, isSeperated, isMaternity)), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult xGetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            bool sa = identity.IsSysAdmin;
            bool ca = identity.IsControlAdmin;
            string userId = identity.UserId;
            string plantId = identity.PlantId;
            string companyGroupId = identity.CompanyGroupId;

            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = " Where (1=0 ";

            if (sa == true || ca == true)
            {
                wcPayrollGroup = @"";
            }
            else
            {
                wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                salaryProcessColumn = "";
                salaryProcessJoin = "";
                wcSalaryProcess = "";
                strDOJ = "AND DOJ<='" + effectiveDate + @"' AND (DOS is null OR DOS>= '" + effectiveDate + @"')";


            }
            else if (!string.IsNullOrEmpty(salaryProcessId))
            {
                salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";

                salaryProcessJoin = @" LEFT  JOIN (
									  SELECT c.* FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + plantId + @"'									  
									  ) SPC ON SPC.EmpInfoSystemID = E.SystemId
                                        LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";

                //salaryProcessJoin = @" LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                //                       LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                //                       left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID  and spd.SalaryProcessId=spm.SystemID   ";

                wcSalaryProcess = @"AND SPC.SlrProcMstSystemID IN('" + salaryProcessId + @"')";

            }
            else if (string.IsNullOrEmpty(salaryProcessId) == true && salaryProcessId != "STRUCTURE")
            {
                salaryProcessColumn = ",ISNULL(SPM.Description,'') SalaryProcess";

                salaryProcessJoin = @" LEFT  JOIN (
									  SELECT c.* FROM SalaryProcChild c 
									  inner join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID and MonthNo =  Month('" + effectiveDate + @"') AND YearNo =  Year('" + effectiveDate + @"')
									  WHERE PlantID = '" + plantId + @"'									  
									  ) SPC ON SPC.EmpInfoSystemID = E.SystemId
                                        LEFT  JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";


                //salaryProcessJoin = @"  LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId
                //                        LEFT OUTER JOIN SalaryProcMaster SPM ON SPM.SystemID = spc.SlrProcMstSystemID and spm.MonthNo = Month('" + effectiveDate + @"') and spm.YearNo = Year('" + effectiveDate + @"')
                //                        left join salaryprocesslogdetail spd on spd.EmpSystemId=SPC.EmpInfoSystemID and spd.SalaryProcessId=spm.SystemID";

                //wcSalaryProcess = @" AND SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                //                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                //                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                //                        AND MonthNo =  MONTH('" + effectiveDate + @"') AND YearNo =  YEAR('" + effectiveDate + @"')  )";
            }
            if (salaryProcessId == "STRUCTURE")
            {
                wcEmpStatus = " Where (1=1 ";
                salaryProcessFlag = "";
            }
            else
            {
                salaryProcessFlag = ", Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag";
                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
            }

            wcEmpStatus += ")";

            var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
            var param = string.Empty;
            if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
            else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                param = "E.GroupID='" + companyGroupId + "'";

            string sql = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId 
	                                ,sl.Id,CheckBoxSelect=case when  sl.Id is null then  CONVERT(bit,0) when sl.IsLocked <> 1  then CONVERT(bit,0) else  CONVERT(bit,1) end   
									,SPM.MonthNo,SPM.YearNo ,sl.IsLocked AS Lock
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName	
                                    , Case when Isnull(SPM.SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end Flag

                                  

	                                 ,(mpb.EntityId) EntityId
									,(mpb.PositionId) PositionId                                     
                                    ,(ld.UserName) Designation                                        
									,(Department.UserName) Department 
									,(Division.UserName) Division 
									,(EmpC.UserName) EmployeeCategory
									,(Plant.UserName) Plant 
									,(Section.UserName) Section 
									,(SubSection.UserName) SubSection 
									,(Unit.UserName) Unit 
                                    ,(eL.UserName) Line

                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    " + salaryProcessFlag + @"
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(spd.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(v.VoucherNo,'' ) VoucherNo
                                    ,ISNULL(sl.PayableVoucherId,'') PayableVoucherId
                                    ,ISNULL(sl.DisbursementVoucherId,'') DisbursementVoucherId
                                    ,v.VoucherNo as PayableVoucherNo
                                    ,vl.VoucherNo as DisbursementVoucherNo
                                    ,SPC.SalaryID as SalaryStructureId
                                    FROM EmployeeInformation e
                                    LEFT OUTER JOIN ORG.Department edept on edept.id=e.DepartmentId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=e.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=e.SubDivisionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                                    LEFT OUTER JOIN ORG.Unit eu on eu.id=e.UnitId
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                  
                                    --LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId,dg.UserName GivenDesignationGroup
									--FROM mst.DesignationMaster dm
									--LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									--) egdsgg on egdsgg.DesignationId=e.GivenDesignationId AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
									LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId                                   
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                           			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = spd.EmployeeCategoryId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=spd.LegalDesignationId
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = spd.BankSystemID
									Left join SalaryLock sl on sl.EmpSystemId=e.SystemId and sl.YearNo=YEAR('" + effectiveDate + @"') AND SL.MonthNo=Month('" + effectiveDate + @"')
                                    LEFT JOIN TRN.Voucher  V ON V.Id=sl.PayableVoucherId 
                                    LEFT JOIN TRN.Voucher  Vl ON Vl.Id=sl.DisbursementVoucherId 

                                     WHERE " + param + @" and isnull(spc.SystemID,'')<>'' " + strDOJ + @" 
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"                                       
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

            JsonResult json = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost]
        public ActionResult Save(List<SalaryLock> EmployeeList, string Month, string Year, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                if (EmployeeList.Count == 0)
                    throw new Exception("Nothing to Lock");
                SaveSalaryLock(EmployeeList, Month, Year);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetLoadSalaryLock(string EmpIdLoop, string Month, string Year, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select * from SalaryLock where  MonthNo='" + Month + @"' and YearNo='" + Year + @"' and EmpSystemId  IN (" + EmpIdLoop + @")";//eee

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

        public void GetSalaryProcDataForControl(string EmpIdLoop, string Month, string Year, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
select DBMA.Id DrControlId,CBMA.Id CrControlId,SL.Id SsalaryLockId,C.* from dbo.SalaryProcChild C
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=C.EmpInfoSystemID
LEFT JOIN ORG.Position PO ON PO.Id=E.PositionID
LEFT JOIN MST.ManpowerBudget DMB ON DMB.Id=E.BudgetCode
left join MST.SalaryHeadGL DGL ON  DGL.SalaryHeadId=C.SalaryHeadID AND DMB.AccountsGroupId=DGL.AccountsGroupId
LEFT JOIN MST.BudgetMasterActivity DBMA ON DBMA.BudgetMasterId=DGL.DrDirectBudgetMasterId AND DBMA.ActivityId=DGL.DrDirectActivityId
left join MST.SalaryHeadGL CGL ON  CGL.SalaryHeadId=C.SalaryHeadID AND DMB.AccountsGroupId=CGL.AccountsGroupId
LEFT JOIN MST.BudgetMasterActivity CBMA ON DBMA.BudgetMasterId=CGL.CrDirectBudgetMasterId AND CBMA.ActivityId=CGL.CrDirectActivityId
JOIN dbo.SalaryLock SL ON SL.EmpSystemId=E.SystemId AND MonthNo=1 AND YearNo=2024
Where C.EmpInfoSystemID IN (" + EmpIdLoop + @") AND PO.DirectManpowerCost=1 AND C.SlrProcMstSystemID IN(Select SystemID  from dbo.SalaryProcMaster Where MonthNo='" + Month + @"' AND YearNo='" + Year + @"') 
UNION 
select DBMA.Id DrControlId,CBMA.Id CrControlId,SL.Id SsalaryLockId,C.* from dbo.SalaryProcChild C
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=C.EmpInfoSystemID
LEFT JOIN ORG.Position PO ON PO.Id=E.PositionID
LEFT JOIN MST.ManpowerBudget DMB ON DMB.Id=E.BudgetCode
left join MST.SalaryHeadGL DGL ON  DGL.SalaryHeadId=C.SalaryHeadID AND DMB.AccountsGroupId=DGL.AccountsGroupId
LEFT JOIN MST.BudgetMasterActivity DBMA ON DBMA.BudgetMasterId=DGL.DrInDirectBudgetMasterId AND DBMA.ActivityId=DGL.DrInDirectActivityId
left join MST.SalaryHeadGL CGL ON  CGL.SalaryHeadId=C.SalaryHeadID AND DMB.AccountsGroupId=CGL.AccountsGroupId
LEFT JOIN MST.BudgetMasterActivity CBMA ON CBMA.BudgetMasterId=CGL.CrInDirectBudgetMasterId AND CBMA.ActivityId=CGL.CrInDirectActivityId
JOIN dbo.SalaryLock SL ON SL.EmpSystemId=E.SystemId AND MonthNo=1 AND YearNo=2024
Where C.EmpInfoSystemID IN (" + EmpIdLoop + @") AND PO.DirectManpowerCost=0 AND C.SlrProcMstSystemID IN(Select SystemID  from dbo.SalaryProcMaster Where MonthNo='" + Month + @"' AND YearNo='" + Year + @"') 
";//eee

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

        private void DetailRow(string OPN_FLAG, int pkCount, string pk_seed, SalaryLock sps, ref DataRow dr)
        {
            string systemID = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (OPN_FLAG == "ADDNEW")
                {
                    systemID = "SL" + pk_seed + "_" + pkCount;
                    dr["Id"] = systemID;
                    dr["EmpSystemId"] = sps.EmpSystemId;
                    dr["YearNo"] = sps.YearNo;
                    dr["MonthNo"] = sps.MonthNo;
                    dr["IsLocked"] = true;
                    dr["GivenDesignationId"] = sps.GivenDesignationId;
                    dr["AccountsGroupId"] = sps.AccountsGroupId;
                    dr["BudgetId"] = sps.BudgetId;
                    dr["SalaryRuleMasterId"] = sps.SalaryRuleMasterId;
                    dr["SalaryStructureId"] = sps.SalaryStructureId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                }
                if (OPN_FLAG == "EDIT")
                {
                    dr["EmpSystemId"] = sps.EmpSystemId;
                    dr["YearNo"] = sps.YearNo;
                    dr["MonthNo"] = sps.MonthNo;
                    dr["IsLocked"] = true;
                    dr["SalaryStructureId"] = sps.SalaryStructureId;
                    dr["GivenDesignationId"] = sps.GivenDesignationId;
                    dr["AccountsGroupId"] = sps.AccountsGroupId;
                    dr["BudgetId"] = sps.BudgetId;
                    dr["SalaryRuleMasterId"] = sps.SalaryRuleMasterId;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        } // end function
        private void SaveSalaryLock(List<SalaryLock> EmployeeList, string Month, string Year)
        {
            DataSet dsSaveSalaryLocked = null;
            DataSet dsSalaryProcData = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            bool DATA_OK = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (DATA_OK == false)
                {
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    string EmpIdLoop = "";
                    string EmpCodeLoop = "";
                    foreach (var item in EmployeeList)
                    {
                        if (EmpIdLoop == "")
                        {
                            EmpIdLoop = "'" + item.EmpSystemId + "'";
                        }
                        else
                        {
                            EmpIdLoop += ",'" + item.EmpSystemId + "'";

                        }
                    }

                    GetLoadSalaryLock(EmpIdLoop, Month, Year, out dsSaveSalaryLocked);
                    dsSaveSalaryLocked.Tables[0].DefaultView.RowFilter = "PayableVoucherId <> '' ";
                    dtLocal = dsSaveSalaryLocked.Tables[0];
                    dvLocal = new DataView();
                    dvLocal.Table = dtLocal;

                    GetSalaryProcDataForControl(EmpIdLoop, Month, Year, out dsSalaryProcData);


                    int _pk_count = 0;
                    string idFromDB = string.Empty;
                    clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SL", out idFromDB);

                    for (int i = 0; i < EmployeeList.Count(); i++)
                    {
                        SalaryLock sd = EmployeeList[i];
                        sd.YearNo = Year;
                        sd.MonthNo = Month;
                        dvLocal.RowFilter = "EmpSystemId='" + sd.EmpSystemId + @"'";

                        if (dvLocal.Count == 0)
                        { // Add new block
                            _pk_count++;
                            drLocal = dtLocal.NewRow();
                            DetailRow("ADDNEW", _pk_count, idFromDB, sd, ref drLocal);
                            dtLocal.Rows.Add(drLocal);
                        }
                        else
                        {//edit block
                            drLocal = dvLocal[0].Row;
                            drLocal.BeginEdit();
                            DetailRow("EDIT", _pk_count, idFromDB, sd, ref drLocal);
                            drLocal.EndEdit();
                        }
                        dvLocal.RowFilter = null;
                    }

                    //dsSaveSalaryLocked.Tables[0].DefaultView.RowFilter = "IsLocked = False";
                    //while (dsSaveSalaryLocked.Tables[0].DefaultView.Count > 0)
                    //{
                    //    dsSaveSalaryLocked.Tables[0].DefaultView[0].Delete();
                    //}


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsSaveSalaryLocked);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                drLocal = null;
                dvLocal = null;
                dtLocal = null;
            }
        }//end of function

        public class SalaryLock : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string EmpSystemId { get; set; }
            public string EmployeeCode { get; set; }
            public string YearNo { get; set; }
            public string MonthNo { get; set; }
            public bool IsLocked { get; set; }
            public bool IsDisbursed { get; set; }
            public string PayableVoucherId { get; set; }
            public string DisbursementVoucherId { get; set; }
            public string Flag { get; set; }
            public string SalaryStructureId { get; set; }
            public string GivenDesignationId { get; set; }
            public string AccountsGroupId { get; set; }
            public string BudgetId { get; set; }
            public string SalaryRuleMasterId { get; set; }
            public bool CheckBoxSelect { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }
        #endregion -- Operations
    }
}