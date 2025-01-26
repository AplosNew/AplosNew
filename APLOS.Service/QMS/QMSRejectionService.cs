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
using System.Net;
using System.Net.Http;


namespace Library.Service.QMS
{
   
        public class QMSRejectionService : Service<QMSRejection>, IQMSRejectionService
    {

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
     //   private readonly IQMSRejectionService _qmsrej;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;
        private readonly IPlantService _plantService;
        private readonly ISignatureService _signatrueService; 
    //    private readonly IRepositoryAsync<QMSRejectionChild> _QMSRejectionChildRepository;


        public QMSRejectionService(
              IRepositoryAsync<QMSRejection> PreRecruitmentEmpReferenceRepositor
            , IPKGeneratorService pkGeneratorService
         //   , IQMSRejectionService qmsrej
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IPlantService plantService
            , ISignatureService signatrueService
         //   , IRepositoryAsync<QMSRejectionChild> QMSRejectionChildRepository

           ) :
            base(PreRecruitmentEmpReferenceRepositor, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _signatrueService = signatrueService; 
          //  _qmsrej = qmsrej;
           // _QMSRejectionChildRepository = QMSRejectionChildRepository;


        }

        #endregion Constructor


        public List<QMSRejection> GetList(string Date, string LocationId)
        {

            string strSql = @"select distinct qmsr.*,PO.Id as POId,Xp.UserName as Customer,p.UserName as Process,sd.Description as Shift,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName,EmpI.EmployeeStatus as EmpIStatus,
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
												 left join MST.CompliedShiftGrouping sd on qmsr.ShiftMasterId=sd.Id 
                                                 where isnull(qmsr.Date,'')='" + Date + "' and isnull(qmsr.LocationId,'')='" + LocationId + "' ";


            return _sqlRepository.GetModelCollection<QMSRejection>(strSql, null);
        }

        public List<QMSRejection> GetDelete(string strkey)
        {

            string strSql = @"select distinct qmsr.*,PO.Id as POId,Xp.UserName as Customer,p.UserName as Process,sd.UserName as Shift,EI.EmployeeStatus,EI.EmployeeCode,EI.EmployeeName as ResponsiblePerson,EmpI.EmployeeCode as EmpCode,EmpI.EmployeeName as EmpName,EmpI.EmployeeStatus as EmpIStatus,
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
												 left join dbo.ShiftDefination sd on qmsr.ShiftMasterId=sd.SystemID 
                                                 where isnull(sd.UserName,'')='" + strkey + "' ";


            return _sqlRepository.GetModelCollection<QMSRejection>(strSql, null);
        }
        
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var _sql = @"select * from TRN.QMSRejection where Id = '" + Id + "' ";
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

        public IEnumerable<object> GetSKUList()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [HKP].[StockKeepingUnit] ";
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

        public IEnumerable<object> GetDefectMasterList()
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

        public IEnumerable<object> GetGradeList()
        {
            try
            {
                var _sql = @"SELECT Id as Value,UserName AS Text FROM [HKP].[GradeMaster] ";
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
         
        public string Create( IEnumerable<QMSRejection> DataToSave)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet dsMaster;
                string TableName = "TRN.QMSRejection";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<QMSRejection> items = DataToSave.ToList();                

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (QMSRejection item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "QR" + _Id;
                        dr["ShiftMasterId"] = item.ShiftMasterId;

                        dr["EmployeeId"] = item.EmployeeId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["LocationId"] = item.LocationId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["ProductionReferenceId"] = item.ProductionReferenceId;
                        dr["Date"] = item.Date;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                  
                }
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                return MasterId;
         
            }


            catch (Exception ex)
            {

                throw(ex);
            }
        }

        public IEnumerable<ComboModel> GetShiftGroupCbo(string plantId)
        {
            var sql = @" select Id,Description UserName from mst.CompliedShiftGrouping where  PlantId='" + plantId + "' ";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<object> GetRejectionMasterId(string MasterId)
        {
            try
            {
                var _sql = @"SELECT Id as Value,Id AS Text FROM TRN.QMSRejection where Id='" + MasterId + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateRejectionChild(IEnumerable<QMSRejectionChild> ChildData, string MasterId)
        {
            try
            {
                DataSet dsMaster;
                string TableName1 = "TRN.QMSRejectionChild";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
               // if (ChildData.Count() == 0)
                 //   return "";

               // List<QMSRejectionChild> items = ChildData.ToList();

              
                //string _Id = "";
                foreach (QMSRejectionChild item in ChildData)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where Id='" + item.Id + "'", out dsMaster, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0 )
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName1, out string _Id);

                        dr["Id"] = "RC" + _Id;
                        dr["QMSRejectionMasterId"] = MasterId;
                        dr["StockKeepingUnitId"] = item.StockKeepingUnitId;
                        dr["QMSDefectMasterId"] = item.QMSDefectMasterId;
                        dr["GradeMasterId"] = item.GradeMasterId;
                        dr["NoOfPics"] = item.NoOfPics;
                        dr["RepairablePics"] = item.RepairablePics;

                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                        clsStaticInfo _info = new clsStaticInfo();

                        _info.SaveDataSets(dsMaster);
                        dsMaster.Tables[0].Rows[0]["Id"] = null;

                    }
                }
            }

            catch (Exception ex)
                 {                           
                           throw (ex);
                  
                 }
                 
        }      


            //private string GetChildPK()
            // {
            //     return GetAutoNumber(nameof(QMSRejectionChild), PKGeneratorEnum.Auto, null, DateTime.Now);
            // }

            // private IEnumerable<QMSRejectionChild> GetQMSRejectionChildList(string RejcPK)
            // {

            //     try
            //     {
            //         string _sql = "select * from TRN.QMSRejectionChild where QMSRejectionMasterId='" + RejcPK + "'";
            //         return _sqlRepository.GetModelCollection<QMSRejectionChild>(_sql, null);
            //     }
            //     catch (Exception)
            //     {
            //         throw;
            //     }
            // }

            //public void SaveChildData(string RejcPK, IEnumerable<QMSRejectionChild> fromUI)
            //{
            //    var flag = false;
            //    try
            //    {
            //        IEnumerable<QMSRejectionChild> fromDB = GetQMSRejectionChildList(RejcPK);
            //        var _pk = GetChildPK();
            //        int _count = 0;
            //        foreach (var ob_ui in fromUI)//if in ui (insert or update)
            //        {
            //            var ob_db = fromDB.Where(r => r.Id == ob_ui.Id).FirstOrDefault();
            //            if (ob_db == null)//not found in db
            //            {
            //                _count++;
            //                ob_ui.Id = "RC" + _pk + "_" + _count;
            //                ob_ui.QMSRejectionMasterId = RejcPK;
            //                ob_ui.ModelState = ModelState.Added;
            //                AuditService.AddedLog(ob_ui);
            //                _QMSRejectionChildRepository.InsertOrUpdateGraph(ob_ui);
            //            }
            //            else
            //            {
            //                //  ob_db.Qty = ob_ui.Qty;
            //                ob_db.ModelState = ModelState.Modified;
            //                AuditService.UpdatedLog(ob_db);
            //                _QMSRejectionChildRepository.InsertOrUpdateGraph(ob_db);
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

            //public string SaveDetail(string MasterId, IEnumerable<QMSRejectionChild> ChildData)
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
            //        throw new CustomException(ex);
            //    }
            //    finally
            //    {
            //        if (flag)
            //            _unitOfWork.Rollback();
            //    }
            //    return "yup";
            //}


            public void Delete(IEnumerable<QMSRejection> DataToDelete)
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
                        objCon.OpenDataSetThroughAdapter("select * from TRN.QMSRejectionChild where QMSRejectionMasterId= '" + item.Id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {

                            objCon.ExecuteNonQueryWrapper("Delete FROM TRN.QMSRejectionChild WHERE QMSRejectionMasterId='" + item.Id + "'", true, "1");
                        }
                    }

                    objCon.ExecuteNonQueryWrapper("Delete FROM TRN.QMSRejection WHERE id='" + item.Id + "'", true, "1");
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

        public IEnumerable<object> GetListRejectionChild(string MasterId)
        {
            try
            {
                var _sql = @"select qmsrc.*,qmsdm.UserName as DefectMaster,sku.UserName as SKU, g.UserName as Grade             
                                 from TRN.QMSRejectionChild qmsrc left join MST.QMSDefectMaster qmsdm on qmsrc.QMSDefectMasterId=qmsdm.Id
                                 left join HKP.StockKeepingUnit sku on qmsrc.StockKeepingUnitId=sku.Id
                                 left join HKP.GradeMaster g on qmsrc.GradeMasterId=g.Id
                                 where QMSRejectionMasterId= '" + MasterId + "' order by DefectMaster ";
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
                var _sql = @"SELECT SystemID as Value,EmployeeName AS Text FROM dbo.EmployeeInformation where EmployeeStatus = 'Active'  AND EmpType!='Guest' and GroupID='" + CompanyGroupId + "' ";
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
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id

                WHERE emp.GroupID = '" + CompanyGroupId + @"' and emp.EmployeeStatus = 'Active'
                 AND isnull(Emp.SystemID,'') not in (select isnull(EmployeeId, '') from TRN.QMSRejection where Id = '" + Id + @"')
                order by EmployeeCodePreFix,EmployeeCodeNumeric";

            return _sqlRepository.GetModelCollection<EmployeeInformation>(strSql, null);
        }
    }
}
