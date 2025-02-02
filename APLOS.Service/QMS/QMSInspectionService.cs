using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.QMS;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using Library.ViewModel.HR;
using System.Data;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Library.Model.Attendances;
using OTSBD;
using Syncfusion.XPS;

namespace Library.Service.QMS
{
   
        public class QMSInspectionService : Service<QMSInspection>, IQMSInspectionService
        {

        #region Constructor
   //     private readonly IQMSInspectionService _qmsip;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;
        private readonly IPlantService _plantService;
        private readonly ISignatureService _signatrueService;
      //  private readonly IRepositoryAsync<QMSInspectionChild> _QMSInspectionChildRepository;


        public QMSInspectionService(
              IRepositoryAsync<QMSInspection> PreRecruitmentEmpReferenceRepositor
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IPlantService plantService
            , ISignatureService signatrueService
         //   , IQMSInspectionService qmsip
           // , IRepositoryAsync<QMSInspectionChild> QMSInspectionChildRepository


           ) :
            base(PreRecruitmentEmpReferenceRepositor, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _signatrueService = signatrueService;
            //_qmsip = qmsip;
          //  _QMSInspectionChildRepository = QMSInspectionChildRepository;

        }

        #endregion Constructor

        public List<QMSInspection> GetList(string Date,string LocationId)
        {

            string strSql = @"select distinct qmsi.*,PO.Id as POId,Xp.UserName as Customer,p.UserName as Process,sd.Description as Shift,it.UserName as InspectionType,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName,EmpI.EmployeeStatus as EmpIStatus,
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
												 left join HKP.InspectionMaster on qmsi.InspectionLevelId=ipm.Id
												 left join MST.CompliedShiftGrouping sd on qmsi.ShiftMasterId=sd.Id
                                                 where isnull(qmsi.Date,'')='" + Date + "' and isnull(qmsi.LocationId,'')='" + LocationId + "' ";


            return _sqlRepository.GetModelCollection<QMSInspection>(strSql, null);
        }

        public List<QMSInspection> GetDelete(string strkey)
        {

            string strSql = @"select distinct qmsr.*,PO.Id as POId,Xp.UserName as Customer,p.UserName as Process,sd.UserName as Shift,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName,EmpI.EmployeeStatus as EmpIStatus,
                                                L.UserName as Location
                                                from TRN.QMSInspection qmsr inner join trn.ProductionOrder PO on qmsr.ProductionReferenceId=PO.Id
		                                        Left JOIN trn.ProductionOrderDetail AS pod ON pod.ProductionOrderId=PO.Id
	                                        	 Left join trn.SalesOrder SO ON SO.Id=POD.SalesOrderId
                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=SO.MasterOrderItemId
                                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                                                left join HKP.Process p on qmsr.ProcessId=p.Id      	 
                                                left join dbo.EmployeeInformation EI on qmsr.ResponsiblePersonId=EI.SystemId
                                                left join dbo.EmployeeInformation EmpI on qmsr.EmployeeId=EmpI.SystemId
	                                        	 left join MST.QMSMaster L on qmsr.LocationId=L.Id
												 left join dbo.ShiftDefination sd on qmsr.ShiftMasterId=sd.SystemID 
                                                 where isnull(sd.UserName,'')='" + strkey + "' ";


            return _sqlRepository.GetModelCollection<QMSInspection>(strSql, null);
        }

        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var _sql = @"select * from TRN.QMSInspection where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetProcess()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM HKP.Process ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetInspectionLevel(string InspectionMasterId)
        {
            try
            {
                var _sql = @"select Id as Value,Category AS Text FROM [hkp].[InspectionMaster] where Id='" + InspectionMasterId + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetInspectionMasterList()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [hkp].[InspectionMaster] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetProductionReference()
        {
            try
            {
                var _sql = @"SELECT Id as Value,Id AS Text FROM [TRN].[ProductionOrder] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetShiftMaster()
        {
            try
            {
                var _sql = @"SELECT SystemID as Value,UserName AS Text FROM [dbo].[ShiftDefination] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<ComboModel> GetShiftGroupCbo(string plantId)
        {
            var sql = @" select Id,Description UserName from mst.CompliedShiftGrouping where  PlantId='" + plantId + "' ";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<object> GetInspectionType()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [hkp].[InspectionType] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetLocationList()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [MST].[QMSMaster] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetStatusList()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [hkp].[QualityStatus]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex )
            {
                throw ex;
            }
        }

        public IEnumerable<object> Getdefectmasterlist()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [MST].[QMSDefectMaster] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> Getdefectzonelist()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [hkp].[DefectZone] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> Getskilllist()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [hkp].[Skill] ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCustomer()
        {
            try
            {
                var _sql = @"select distinct Xp.Id as Value,Xp.UserName as Customer
                                                from trn.ProductionOrderDetail AS pod left join trn.ProductionOrder PO ON pod.ProductionOrderId=PO.Id
	                                        	 Left join trn.SalesOrder SO ON SO.Id=POD.SalesOrderId
                                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=SO.MasterOrderItemId
                                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string Create(IEnumerable<QMSInspection> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "TRN.QMSInspection";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<QMSInspection> items = DataToSave.ToList();
                                
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (QMSInspection item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "QI" + _Id;
                        dr["InspectionMasterId"] = item.InspectionMasterId;
                        dr["InspectionTypeId"] = item.InspectionTypeId;
                        dr["InspectionLevelId"] = item.InspectionLevelId;
                        dr["EmployeeId"] = item.EmployeeId;
                        dr["Date"] = item.Date;
                        dr["ShiftMasterId"] = item.ShiftMasterId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["LocationId"] = item.LocationId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["ProductionReferenceId"] = item.ProductionReferenceId;
                        dr["BatchReferenceNo"] = item.BatchReferenceNo;
                        dr["BatchSize"] = item.BatchSize;
                        dr["SampleSize"] = item.SampleSize;
                        dr["NoOfDefectiveUnit"] = item.NoOfDefectiveUnit;
                        dr["StatusId"] = item.StatusId;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["InspectionMasterId"] = item.InspectionMasterId;
                        dr["InspectionTypeId"] = item.InspectionTypeId;
                        dr["InspectionLevelId"] = item.InspectionLevelId;
                        dr["EmployeeId"] = item.EmployeeId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["LocationId"] = item.LocationId;
                        dr["ShiftMasterId"] = item.ShiftMasterId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["ProductionReferenceId"] = item.ProductionReferenceId;
                        dr["BatchReferenceNo"] = item.BatchReferenceNo;
                        dr["BatchSize"] = item.BatchSize;
                        dr["SampleSize"] = item.SampleSize;
                        dr["NoOfDefectiveUnit"] = item.NoOfDefectiveUnit;
                        dr["StatusId"] = item.StatusId;
                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;
                        dr["Date"] = item.Date;
                        dr.EndEdit();
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;

            }


            catch (Exception ex)
            {

                throw (ex);
            }
        }
        public void CreateInspectionChild(IEnumerable<QMSInspectionChild> ChildData, string MasterId)
        {

            try
            {
                DataSet dsMaster;
                string TableName1 = "TRN.QMSInspectionChild";


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
              //  if (ChildData.Count() == 0)
                  //  return "";
             //   List<QMSInspectionChild> items = ChildData.ToList();
                           //  string _Id = "";

                foreach (QMSInspectionChild item in ChildData)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + item.Id + "'", out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName1, out string _Id);

                        dr["Id"] = "IC" + _Id;
                        dr["QMSInspectionId"] = MasterId;
                        dr["QMSDefectMasterId"] = item.QMSDefectMasterId;
                        dr["QMSDefectZoneId"] = item.QMSDefectZoneId;
                        dr["MajorMinor"] = item.MajorMinor;
                        dr["NoOfDefect"] = item.NoOfDefect;
                        dr["SkillId"] = item.SkillId;
                        dr["DefectResponsiblePersonId"] = item.DefectResponsiblePersonId;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dsMaster.Tables[0].Rows.Add(dr);

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                                                 
                    }                      
                }
               // string QMSInspectionId = dsMaster.Tables[0].Rows[0]["QMSInspectionId"].ToString();
               // return QMSInspectionId;

            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private string GetChildPK()
        {
            return GetAutoNumber(nameof(QMSInspectionChild), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private IEnumerable<QMSInspectionChild> GetQMSInspectionChildList(string InspcPK)
        {

            try
            {
                string _sql = "select * from TRN.QMSInspectionChild where QMSInspectionId='" + InspcPK + "'";
                return _sqlRepository.GetModelCollection<QMSInspectionChild>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public void SaveChildData(string InspcPK, IEnumerable<QMSInspectionChild> fromUI)
        //{
        //    var flag = false;
        //    try
        //    {
        //        IEnumerable<QMSInspectionChild> fromDB = GetQMSInspectionChildList(InspcPK);
        //        var _pk = GetChildPK();
        //        int _count = 0;
        //        foreach (var ob_ui in fromUI)//if in ui (insert or update)
        //        {
        //            var ob_db = fromDB.Where(r => r.Id == ob_ui.Id).FirstOrDefault();
        //            if (ob_db == null)//not found in db
        //            {
        //                _count++;
        //                ob_ui.Id = "IC" + _pk + "_" + _count;
        //                ob_ui.QMSInspectionId = InspcPK;
        //                ob_ui.ModelState = ModelState.Added;
        //                AuditService.AddedLog(ob_ui);
        //                _QMSInspectionChildRepository.InsertOrUpdateGraph(ob_ui);
        //            }
        //            else
        //            {
        //                //  ob_db.Qty = ob_ui.Qty;
        //                ob_db.ModelState = ModelState.Modified;
        //                AuditService.UpdatedLog(ob_db);
        //                _QMSInspectionChildRepository.InsertOrUpdateGraph(ob_db);
        //            }

        //        }
        //    }
        //    catch (CustomException ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
        //    }
        //}

        //public string SaveDetail(string MasterId, IEnumerable<QMSInspectionChild> ChildData)
        //{
        //    var flag = false;
        //    try
        //    {
        //        _unitOfWork.BeginTransaction();
        //        flag = true;

        //        SaveChildData(MasterId, ChildData);

        //        _unitOfWork.SaveChanges();
        //        flag = false;
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //      //  throw new CustomException(ex);
        //    }
        //    finally
        //    {
        //        if (flag)
        //            _unitOfWork.Rollback();
                
        //    }
        //    return "yup";
        //}




        public void Delete(IEnumerable<QMSInspection> DataToDelete)
        {


            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                DataSet dsMaster;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                foreach (var item in DataToDelete)
                {
                    if (!string.IsNullOrEmpty(item.Id))
                    {
                        objCon.OpenDataSetThroughAdapter("select * from TRN.QMSInspectionChild where QMSInspectionId= '" + item.Id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            objCon.ExecuteNonQueryWrapper("Delete FROM TRN.QMSInspectionChild WHERE QMSInspectionId='" + item.Id + "'", true, "1");
                        }
                    }

                    objCon.ExecuteNonQueryWrapper("Delete FROM TRN.QMSInspection WHERE id='" + item.Id + "'", true, "1");
                }

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }

        }//end of function

        public IEnumerable<object> GetListInspectionChild(string QMSInspectionId)
        {
            try
            {
                var _sql = @"select qmsic.*,qmsdm.UserName as DefectMaster,dz.UserName as DefectZone, s.UserName as Skill,einfo.EmployeeName as DefResPonName,
                                 einfo.EmployeeCode as DefResPonCode,einfo.EmployeeStatus as EmpICStatus
                                 from TRN.QMSInspectionChild qmsic left join MST.QMSDefectMaster qmsdm on qmsic.QMSDefectMasterId=qmsdm.Id
                                 left join HKP.DefectZone dz on qmsic.QMSDefectZoneId=dz.Id
                                 left join HKP.Skill s on qmsic.SkillId=s.Id
								 left join dbo.EmployeeInformation einfo on qmsic.DefectResponsiblePersonId=einfo.SystemId
                                 where QMSInspectionId= '" + QMSInspectionId + "' order by DefectMaster ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
               
        public IEnumerable<object> LoadAllResPersonDetailsForSelection(string CompanyGroupId)
        {
            try
            {
                var _sql = @"SELECT SystemID as Value,EmployeeName AS Text,EmployeeCode as Code FROM dbo.EmployeeInformation where EmployeeStatus = 'Active'  AND EmpType!='Guest' and GroupID='" + CompanyGroupId + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<EmployeeInformation> LoadAllEmpDetailsForSelection(string CompanyGroupId, string Id)
        {
            string strSql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
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

                WHERE emp.GroupID = '" + CompanyGroupId + @"' and emp.EmployeeStatus = 'Active'
                 AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId, '') from TRN.QMSInspection where Id = '" + Id + @"')
                order by EmployeeCodePreFix,EmployeeCodeNumeric";

            return _sqlRepository.GetModelCollection<EmployeeInformation>(strSql, null);
        }

        public List<EmployeeInformation> LoadAllDefResPonDetailsForSelection(string CompanyGroupId, string Id)
        {
            string strSql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
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

                WHERE emp.GroupID = '" + CompanyGroupId + @"' and emp.EmployeeStatus = 'Active'
                 AND isnull(Emp.SystemID,'') not in (select isnull(DefectResponsiblePersonId, '') from TRN.QMSInspectionChild where Id = '" + Id + @"')
                order by EmployeeCodePreFix,EmployeeCodeNumeric";

            return _sqlRepository.GetModelCollection<EmployeeInformation>(strSql, null);
        }


    }
}
