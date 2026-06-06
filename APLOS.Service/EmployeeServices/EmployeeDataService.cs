using System;
using System.Collections.Generic;
using System.Linq;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.EmployeeServices;
using Library.Service.Core;
using Library.Service.Organizations;
using Library.Service.Systems;
using System.Data;
using OTSBD;

namespace Library.Service.EmployeeServices
{

    public class EmployeeDataService : Service<EmployeeData>, IEmployeeDataService
    {

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pk;
        private readonly IPlantService _plantService;
        private readonly ISignatureService _signatrueService;


        public EmployeeDataService(
              IRepositoryAsync<EmployeeData> PreRecruitmentEmpReferenceRepositor
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IPlantService plantService
            , ISignatureService signatrueService

           ) :
            base(PreRecruitmentEmpReferenceRepositor, unitOfWork, pkGeneratorService)
        {
            _pk = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _plantService = plantService;
            _signatrueService = signatrueService;


        }

        #endregion Constructor

        public string Delete(IEnumerable<EmployeeData> DataToDelete)
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
                        objCon.OpenDataSetThroughAdapter("select * from dbo.EmpServiceData where Id= '" + item.Id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            objCon.ExecuteNonQueryWrapper("Delete FROM dbo.EmpServiceData where Id='" + item.Id + "'", true, "1");
                        }
                    }

                }

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                return ex.ToString();

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
            return "";
        }//end of function


        public IEnumerable<object> GetList(string AddedBy)
        {


            string sql = @"select ed.*,EI.EmployeeCode,esc.Category,est.Service from dbo.EmpServiceData ed left join dbo.EmployeeInformation EI on ed.EmployeeId = EI.SystemId
                                      left join dbo.EmpServiceCategory esc on esc.Id = ed.EmployeeServiceCategoryId
                                      left join dbo.EmpServiceType est on est.Id = esc.EmpServiceTypeId
                                      where isnull(ed.AddedBy,'')='" + AddedBy + "' and ed.Date between(SELECT DATEADD(month, -1, CAST(GETDATE() AS Date))) and (select CAST(GETDATE() AS Date))";



            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetShiftMaster(string PlantId)
        {
            try
            {
                var _sql = @"SELECT distinct SystemID as Value,UserName AS Text FROM [dbo].[ShiftDefination] where isnull(PlantID,'')='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string Create(IEnumerable<EmployeeData> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "dbo.EmpServiceData";

                ConnectionManager.DAL.ConManager con =
                    new ConnectionManager.DAL.ConManager("1");

                if (DataToSave == null || !DataToSave.Any())
                    return "";

                List<EmployeeData> items = DataToSave.ToList();

                // Empty structure only
                con.OpenDataSetThroughAdapter(
                    "select * from " + TableName + " where 1=2",
                    out dsMaster,
                    false,
                    "1");

                string lastInsertedId = "";

                foreach (EmployeeData item in items)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    bplib.clsGenID genid = new bplib.clsGenID();
                    string _Id = "";

                    genid.GenID(TableName, out _Id);

                    lastInsertedId = "ED" + _Id;

                    dr["Id"] = lastInsertedId;
                    dr["EmployeeId"] = item.EmployeeId;
                    dr["Date"] = item.Date;
                    dr["Time"] = item.Time;
                    dr["ShiftId"] = item.ShiftId;
                    dr["EmployeeServiceCategoryId"] = item.EmployeeServiceCategoryId;
                    dr["Chargeable"] = item.Chargeable;
                    dr["IsProcessed"] = false;
                    dr["From"] = item.From;
                    dr["To"] = item.To;
                    dr["Quantity"] = item.Quantity;
                    dr["Particulars"] = item.Particulars;
                    dr["BillOtherReferenceNo"] = item.BillOtherReferenceNo;
                    dr["Amount"] = item.Amount;

                    dr["AddedBy"] = item.AddedBy;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = item.AddedFromIP;

                    dsMaster.Tables[0].Rows.Add(dr);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return lastInsertedId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<object> EmpCodeId(string CompanyGroupId)
        {
            try
            {
                var _sql = @"SELECT EmployeeCode as Code,SystemID as Value FROM dbo.EmployeeInformation where EmployeeStatus = 'Active' AND EmpType!='Guest' and GroupID='" + CompanyGroupId + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetCount(string EmpId, string Service)
        {
            try
            {
                string sql = @"select Count(t.Service) as Value,t.Service as Text from 
                dbo.EmpServiceData emp left join dbo.EmpServiceCategory ex on ex.Id=emp.EmployeeServiceCategoryId
                left join dbo.EmpServiceType t on t.Id=ex.EmpServiceTypeId where
                emp.EmployeeId='" + EmpId + "' and t.Service='" + Service + "' group by t.Service";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpType(string EmpId)
        {
            try
            {
                string sql = @"select EmpType from dbo.EmployeeInformation where SystemId='" + EmpId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDeduction(string EmpId, string Service)
        {
            try
            {
                var sql = @"select emp.Id,t.Service,ei.EmployeeName as AddedBy,emp.EmployeeId,
                emp.AddedDate,emp.AddedBy as AddedId,emp.Amount,emp.BillOtherReferenceNo,emp.Quantity,
                ex.Category,emp.Particulars from dbo.EmpServiceData emp left join dbo.EmpServiceCategory ex 
                on ex.Id=emp.EmployeeServiceCategoryId
                left join EmployeeInformation ei on ei.SystemId=emp.AddedBy
                left join dbo.EmpServiceType t on t.Id=ex.EmpServiceTypeId where emp.EmployeeId='" + EmpId + "' and " +
                "t.Service='" + Service + "' order by (emp.AddedDate) desc";
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetUpdatedDeduction(string EmpId, string Service)
        {
            try
            {
                var sql = @"select emp.Id,t.Service,emp.EmployeeId,
                emp.AddedDate,emp.AddedBy,emp.Amount,emp.BillOtherReferenceNo,emp.Quantity,
                ex.Category,emp.Particulars from dbo.EmpServiceData emp left join dbo.EmpServiceCategory ex 
                on ex.Id=emp.EmployeeServiceCategoryId
               left join dbo.EmpServiceType t on t.Id=ex.EmpServiceTypeId where emp.EmployeeId='" + EmpId + "'" +
               " and t.Service='" + Service + "' order by (emp.AddedDate) desc";
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
