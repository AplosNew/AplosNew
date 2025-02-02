using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
    public class IndividualGratuityPolicy
    {
        private SqlRepository _sqlRepository;

        public IndividualGratuityPolicy()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetGratuityIns()
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select Id, AgreementNo from GratuityInsuranceAgreement";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void SaveMaster(List<EmpList> EmpList)
        {

            try
            {

                DataSet dsEmpList;

                GetIndividualGP(EmpList, out dsEmpList);

                _IndividualGP(ref dsEmpList, EmpList);


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsEmpList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetIndividualGP(List<EmpList> EmpList, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _Id = string.Empty;
            try
            {
                foreach (var item in EmpList)
                {
                    if (item.Id != null)
                    {

                        if (_Id == "")
                        {
                            _Id = "'" + item.Id.Replace(",", "','") + "'";
                        }
                        else
                        {
                            _Id += ",'" + item.Id.Replace(",", "','") + "'";
                        }
                    }
                    //_Id = "'',"'"+item.Id+"'
                }
                if (_Id != "")
                {
                    strSQL = "SELECT * FROM dbo.IndividualGratuityPolicy WHERE Id in (" + _Id + ")";
                }
                else
                {
                    strSQL = "SELECT * FROM dbo.IndividualGratuityPolicy ";
                }

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

        void _IndividualGP(ref DataSet dsSaveBonusMonths, List<EmpList> List)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            try
            {
                string seed_detail = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Individual_GP", out seed_detail);
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in List)
                {
                    dvMSave = new DataView();
                    dvMSave.Table = dtMSave;
                    dvMSave.RowFilter = "Id ='" + item.Id + "' ";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        string pk = "IGP_" + seed_detail + "_" + count;
                        drMSave = dtMSave.NewRow();
                        drMSave["Id"] = pk;
                        drMSave["EmployeeSystemId"] = item.EmployeeSystemId;
                        drMSave["TenureYear"] = item.TYear;
                        drMSave["TenureMonth"] = item.TMonth;
                        drMSave["PolicyNo"] = item.PolicyNo;
                        drMSave["AgreementId"] = item.AgreementId;

                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;
                        dtMSave.Rows.Add(drMSave);
                    }
                    else
                    {
                        drMSave = dvMSave[0].Row;
                        drMSave.BeginEdit();
                        drMSave["TenureYear"] = item.TYear;
                        drMSave["TenureMonth"] = item.TMonth;
                        drMSave["PolicyNo"] = item.PolicyNo;
                        drMSave["AgreementId"] = item.AgreementId;

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        drMSave.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetList(string EmpSytemIDList,string PlantId)
        {


            try
            {
                string strSQL = string.Empty;
                strSQL = @"select igp.*,EMP.EmployeeCode,emp.EmployeeName,emp.FatherName,
                                format (emp.DOB,'dd-MMM-yyyy') DOB,format (emp.DOJ,'dd-MMM-yyyy')DOJ, LDEG.UserName Designation
                                from [dbo].[IndividualGratuityPolicy] igp
                                left join EmployeeInformation EMP on emp.SystemId = igp.EmployeeSystemId
                                LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                where igp.AgreementId <> '' and emp.PlantId='"+ PlantId + @"'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetGPDetails(string plantID)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select case when m.IsRoudingSixMonth = '1' then 'Rouding Six Month is Applicable' else 'Rouding Six Month is Not Applicable' end IsRound,d.MaturityFromYear,d.MaturityToYear
                            from GratuityPolicyMaster m
                            left join GratuityPolicyDetails d on d.GratuityPolicyMasterId = m.Id
                            where m.plantId ='" + plantID + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetEmployeeList(string plantId, string companyId)
        {
            try
            {
                string CmdText = @"select * from (SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,                                       
										EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,										
										(DATEADD(month, convert(float, gd.MaturityFromYear)*12, EMP.DOJ)) TValue,
									    DATEADD(day,1,	(DATEADD(month, (convert(float, gd.MaturityFromYear)*12)-6, EMP.DOJ))) TValue2,
									    kk=case when gm.IsRoudingSixMonth = 1 then  DATEADD(day,1,	(DATEADD(month, (convert(float, gd.MaturityFromYear)*12)-6, EMP.DOJ)))
									    else (DATEADD(month, convert(float, gd.MaturityFromYear)*12, EMP.DOJ)) end  ,                                      
										EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
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
                                        left join GratuityPolicyMaster gm on gm.plantId = EMP.PlantId
										left join GratuityPolicyDetails gd on gd.GratuityPolicyMasterId = gm.Id
                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' --and EMP.EmployeeStatus='Active'
                                        ) hh
									    where	kk <= GETDATE()
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

    }
}

public class EmpList
{
    public string Id { get; set; }
    public string EmployeeSystemId { get; set; }
    public string TYear { get; set; }
    public string TMonth { get; set; }
    public string PolicyNo { get; set; }
    public string AgreementId { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public string AddedFromIP { get; set; }
    public string UpdatedFromIP { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}