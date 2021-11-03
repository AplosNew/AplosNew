using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.WorkCenters;
using Library.Service.Core;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Library.Service.WorkCenters
{
    public class WorkCenterMasterMachineService : Service<WorkCenterMasterMachine>, IWorkCenterMasterMachineService
    {
        #region Table Name

        private readonly string tWorkCenterMaster = " " + DbSchema.SystemConfigurationAndSetup + ".[WorkCenterMaster] ";
        private readonly string tWorkCenterMasterMachine = " " + DbSchema.SystemConfigurationAndSetup + ".[WorkCenterMasterMachine] ";
        private readonly string tFixedAssetSubCategory = " " + DbSchema.HKP + ".[FixedAssetSubCategory] ";
        private readonly string tFixedAssetCategory = " " + DbSchema.HKP + ".[FixedAssetCategory] ";
        private readonly string tFixedAsset = " " + DbSchema.HKP + ".[FixedAsset] ";
        private readonly string tCountry = " " + DbSchema.SystemConfigurationAndSetup + ".[Country] ";
        private readonly string tParty = " " + DbSchema.HKP + ".[Party] ";//SCS.HKP.
        private readonly string tFixedAssetItem = " " + DbSchema.Transaction + ".[MaterialMasterMachineProcess] ";
        private readonly string tMachineClass = " " + DbSchema.HKP + ".[" + DbTable.MachineClass + "] ";
        private readonly string tFixedAssetMasterMachineType = " " + DbSchema.Masters + ".[FixedAssetMasterMachineType] ";

        #endregion Table Name

        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;//
        private readonly IRepositoryAsync<WorkCenterMasterMachine> _workCenterMasterMachineRepository;

        public WorkCenterMasterMachineService(
            IRepositoryAsync<WorkCenterMasterMachine> workCenterMasterMachineRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(workCenterMasterMachineRepository, unitOfWork, pkGeneratorService)
        {
            _workCenterMasterMachineRepository = workCenterMasterMachineRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public string GetPK()
        {
            return "WM" + _pkGeneratorService.GetAutoNumber("WorkCenterMasterMachine", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public WorkCenterMasterMachine GetMaster(string PK)
        {
            try
            {
                var _sql = "select * from " + tWorkCenterMasterMachine + " where Id='" + PK + "'";
                return _workCenterMasterMachineRepository.SelectQuery(_sql).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<WorkCenterMasterMachine> GetDetailListByMasterId(string WorkCenterMasterId)
        {
            try
            {
                var _sql = "select * from " + tWorkCenterMasterMachine + " where WorkCenterMasterId='" + WorkCenterMasterId + "'";
                return _sqlRepository.GetModelCollection<WorkCenterMasterMachine>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string masterid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"
                                  select
                                m.WorkCenterMasterId,m.FixedAssetItemId,am.SerialNo,s.UserName FixedAsset
                                ,t.[Description] MachineType,c.UserName MachineClass,0 Archive,m.Id
                                 from " + tWorkCenterMasterMachine + @" m
                                left outer join " + tFixedAssetItem + @" am on am.Id=m.FixedAssetItemId
                                LEFT outer JOIN " + tFixedAsset + @" s ON s.Id = am.FixedAssetId
                                left outer join " + tFixedAssetMasterMachineType + @" a on a.FixedAssetItemId=am.Id
                                left outer join " + tMachineClass + @" c on c.Id=t.MachineClassId
                                WHERE m.WorkCenterMasterId='" + masterid + @"'
                                Order by am.SerialNo";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetDetailListByPlant(string plantid, string currentMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"
                                SELECT m.Id
	                                ,m.Code
	                                ,m.UserName WorkCenter
	                                ,d.FixedAssetItemId
	                                ,f.UserName
	                                ,f.Code
	                                ,t.[Description] MachineType
	                                ,c.UserName MachineClass
	                                ,a.SerialNo
                                FROM " + tWorkCenterMaster + @" m
                                LEFT JOIN " + tWorkCenterMasterMachine + @" d ON m.Id = d.WorkCenterMasterId
                                LEFT JOIN " + tFixedAssetItem + @" a ON d.FixedAssetItemId = a.Id
                                LEFT JOIN " + tFixedAsset + @" f ON a.FixedAssetId = f.Id
                                LEFT JOIN " + tFixedAssetMasterMachineType + @" amt ON a.Id = amt.FixedAssetItemId
                                LEFT JOIN " + tMachineClass + @" c ON t.MachineClassId = c.Id
                                WHERE m.CompanyId = '" + identity.CompanyId + "' and m.PlantId='" + plantid + @"' and m.Id<>'" + currentMasterId + @"' and isnull(d.Id,'')<>''
                                Order by m.Code";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"
                                SELECT m.Id
                                    ,s.UserName FixedAsset
                                    ,c.UserName FixedAssetCategory
	                                ,sc.UserName FixedAssetSubcategory
                                    ,p.UserName Vendor
                                    ,m.SerialNo
                                    ,Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate
                                    ,m.Brand
	                                ,m.InvoiceNo
	                                ,mt.[Description] MachineType
	                                ,m.Model
	                                ,m.YearOfManufacture
	                                ,m.YearOfInstallation
	                                ,cn.UserName Country
	                                ,m.IsForProduction

                                FROM " + tWorkCenterMasterMachine + @" m
                                LEFT outer JOIN " + tFixedAsset + @" s ON s.Id = m.FixedAssetId
                                LEFT outer JOIN " + tFixedAssetCategory + @" c ON c.Id = m.FixedAssetCategoryId
                                LEFT outer JOIN " + tFixedAssetSubCategory + @" sc ON sc.Id = m.FixedAssetSubCategoryId
                                LEFT outer JOIN " + tCountry + @"  cn ON cn.Id = m.CountryOfOriginId
                                LEFT outer JOIN " + tFixedAssetMasterMachineType + @"  fm ON fm.FixedAssetItemId = m.Id
                                left outer join " + tParty + @" p on p.Id=m.VendorId

                                WHERE m.CompanyId = '" + identity.CompanyId + @"'  and m.Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Insert n Update

        private void GetDetailList(string MasterId, string PlantId, IEnumerable<WorkCenterMasterMachine> from_ui, out List<WorkCenterMasterMachine> from_db)
        {
            var _count = 0;
            from_db = null;
            IEnumerable<object> from_db_all_list = null;
            try
            {
                from_db = GetDetailListByMasterId(MasterId).ToList<WorkCenterMasterMachine>();
                from_db_all_list = GetDetailListByPlant(PlantId, MasterId);
                if (from_db == null)
                {
                    from_db = new List<WorkCenterMasterMachine>();
                }
                Validation(from_ui, from_db_all_list);
                var _PK = GetPK();
                foreach (var ui in from_ui)
                {
                    var db = from_db.FirstOrDefault(a => a.Id == ui.Id);
                    if (db == null)//new
                    {
                        if (!ui.Archive)
                        {
                            _count += 1;
                            db = new WorkCenterMasterMachine
                            {
                                Id = _PK + "-" + _count,
                                ModelState = ModelState.Added
                            };
                            AuditService.Log(db);

                            db.WorkCenterMasterId = MasterId;
                            db.FixedAssetItemId = ui.FixedAssetItemId;
                            from_db.Add(db);
                        }
                    }
                    else
                    {
                        if (!ui.Archive)
                        {
                            db.ModelState = ModelState.Modified;
                            AuditService.Log(db);

                            db.WorkCenterMasterId = MasterId;
                            db.FixedAssetItemId = ui.FixedAssetItemId;
                        }
                        else
                        {
                            db.ModelState = ModelState.Deleted;
                            AuditService.Log(db);
                        }
                    }
                }//foreach
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdatedetail(string masterid, string PlantId, IEnumerable<WorkCenterMasterMachine> from_ui)
        {
            List<WorkCenterMasterMachine> localMaster = null;
            //masterid = string.Empty;

            var flag = false;
            try
            {
                GetDetailList(masterid, PlantId, from_ui, out localMaster);
                foreach (var ui in localMaster)
                {
                    InsertOrUpdateGraph(ui);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void Validation(IEnumerable<WorkCenterMasterMachine> detail_ui, IEnumerable<object> from_db_List)
        {
            try
            {
                foreach (var item in detail_ui)
                {
                    if (!item.Archive)
                    {
                        CheckMachineDuplication(item.FixedAssetItemId, from_db_List);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CheckMachineDuplication(string machineid, IEnumerable<object> from_db_List)
        {
            try
            {
                foreach (var item in from_db_List)
                {
                    var dic = (Dictionary<string, object>)item;
                    if (dic["FixedAssetItemId"].ToString() == machineid)
                    {
                        throw new Exception("Serial: [" + dic["SerialNo"]+ "], Fixed Asset: [" + dic["FixedAsset"]+ "], Machine Type: [" + dic["MachineType"]+ "], Machine Class: [" + dic["MachineClass"]+ "] has already been tagged with Work Center: [" + dic["WorkCenter"]+ "]...");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Insert n Update

        #region Delete

        public void DeleteMaster(string masterid)
        {
            WorkCenterMasterMachine from_db = null;
            var flag = false;
            try
            {
                //master
                DelMaster(masterid, out from_db);

                Delete(from_db);
                //machinetype

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void DelMaster(string id, out WorkCenterMasterMachine from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetMaster(id);

                if (from_db.Id == null || from_db.Id == "")
                {
                    throw new Exception("No Machine found against Id: [" + id + "]");
                }
                else
                {
                    from_db.ModelState = ModelState.Deleted;
                    //AuditService.Log(from_db, true);
                    // from_db.Archive = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Delete
    }
}