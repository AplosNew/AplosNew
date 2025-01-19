using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Library.ViewModel.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class ShiftAssignmentDeleteController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public ShiftAssignmentDeleteController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
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
        public ActionResult Delete(string pFromDate, string[] EmpList)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string EmpIdLoop = "";
                DataSet dsDateWiseShiftAssign;
                DataSet dsMEmployeeShiftAssign;
                foreach (string item in EmpList)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item + "'";

                    }
                }

                //DataSet dsMaster;
                //GetPWHRMData(out dsMaster);
                //var db = dsMaster.Tables[0].ToList<MasterData>();

                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                DateTime FromDate = Convert.ToDateTime(pFromDate);
                DateTime pToDate = DateTime.Now;
                DateTime ToDate = Convert.ToDateTime(pToDate);
                while (FromDate <= ToDate)
                {
                    if (EmpIdLoop.Length > 0)
                    {
                        obj.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpIdLoop);
                    }
                    FromDate = FromDate.AddDays(1);
                }

                string sql2 = @"delete from AttdnProcessData where EmpSystemID in (" + EmpIdLoop + @") and WorkDate >='" + pFromDate + "' ";
                ExecuteRawSQL(sql2);
                string sql = @"delete from [dbo].[EmpDateWiseShiftAssign] where  EmpSystemID in (" + EmpIdLoop + @") and WorkDate >= '" + pFromDate + "' ";
                string sql1 = @"delete from [dbo].[EmployeeShiftAssign] where  EmpSystemID in (" + EmpIdLoop + @") and EffectiveDate >= '" + pFromDate + "' and IsSingleDayShift=0 ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDateWiseShiftAssign, false, "1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsMEmployeeShiftAssign, false, "1");

                #region Attendance process

            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        public void ExecuteRawSQL(string sql1)
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
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
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
                    objCon.CloseConnection();
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function

        [HttpPost]
        public ActionResult GetEmpInfo(string fromDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyGroupId = identity.CompanyGroupId;
                string plantId = identity.PlantId;
                string wcManual = "";
                string Apjoin = "";
                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND E.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var OrgStrList = OrgStructureList(companyGroupId);
                foreach (var item in OrgStrList)
                {
                    if (item.RType == "Entity")
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";

                        if (item.ColumnName == "EmployeeGroup")
                        {
                            Join += "LEFT JOIN [HKP].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n";
                        }
                        else
                        {
                            Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = EN." + item.ColumnName + "Id\n";
                        }
                    }
                    else
                    {
                        cList += "," + item.ColumnName + ".UserName " + item.ColumnName + " ";
                        Join += "LEFT JOIN [ORG].[" + item.ColumnName + "] ON " + item.ColumnName + ".Id = PO." + item.ColumnName + "Id\n";
                    }
                }
                var cmdText = @"SELECT    * fROM(  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'false'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(ld.UserName,egdsg.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus 
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,E.PlantId
	                                ,x.EffectiveDate
								    ,x.ShiftSystem
								    ,shiftd.ShiftDefinationName
								    ,X.SingleDayShift
                                    FROM EmployeeInformation e

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                    LEFT OUTER JOIN ORG.Department edept on edept.id=PO.DepartmentId
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId
                                    LEFT OUTER JOIN ORG.Division ediv on ediv.id=PO.DivisionId
                                    LEFT OUTER JOIN ORG.SubDivision esdiv on esdiv.id=PO.SubDivisionId
                                    LEFT OUTER JOIN ORG.Section es on es.id=PO.SectionId
                                    LEFT OUTER JOIN ORG.SubSection ess on ess.id=PO.SubSectionId
                                    LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
                                    
                                 
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    " + Apjoin + @"
                                    LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
  		                                -------SUB-----
									left join ( select ESA.EmpSystemID,ESA.FixSystemID,ESA.IsSingleDayShift,format(ESA.EffectiveDate,'dd-MMM-yyyy')as EffectiveDate
									,ShiftSystem= case when isnull(esa.FixSystemID,'')=''  then RosterStartShiftID else FixSystemID end 
									,SingleDayShift= CASE WHEN esa.IsSingleDayShift=1 THEN 'Yes' else 'No' 
									end From EmployeeShiftAssign AS ESA
									INNER JOIN (select max(EffectiveDate)AS MaxEffectiveDate,EmpSystemID From EmployeeShiftAssign where IsSingleDayShift=0 group by EmpSystemID)
									 AS Y ON Y.EmpSystemID =ESA.EmpSystemID AND ESA.EffectiveDate=Y.MaxEffectiveDate ) as x on x.EmpSystemID=e.SystemId 
										left join ShiftDefination shiftd on shiftd.SystemID=x.ShiftSystem
									    ------SUB-------
                                  WHERE                   
                                   e.plantId='" + plantId + @"' and e.GroupID='" + companyGroupId+ @"'    and    X.IsSingleDayShift=0 and             
                                     CONVERT(date,x.EffectiveDate) >= '" + fromDate + "' " + wcManual + @"
                                     ) DD ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";               
                JsonResult json = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId)
        {
            try
            {
                var strSQL = @" SELECT DISTINCT u.StandardName ColumnName,IsNULL(e.RType,'position') as Rtype,e.Sequence eSequence,p.Sequence pSequence from (
                                SELECT DISTINCT StandardName from [ORG].[StructureRelationship] as ee where CompanyGroupId='" + CompanyGroupId + @"' and RType = 'Entity' union
                                SELECT DISTINCT StandardName from [ORG].[StructureRelationship] as pp where CompanyGroupId='" + CompanyGroupId + @"' and RType = 'position' ) u
                                LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Entity' ) e on e.StandardName = u.StandardName
                                LEFT OUTER JOIN(SELECT id,StandardName,RType,Sequence from [org].StructureRelationship where RType='Position' ) p on p.StandardName = u.StandardName";
                return _sqlRepository.GetModelCollection<OrgStructureListViewModel>(strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public void GetPWHRMData(out DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = "select * from PlantWiseHRMSSetting ";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function

        #endregion -- Operations  
    }
}