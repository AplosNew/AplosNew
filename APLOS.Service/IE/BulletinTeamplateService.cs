using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Library.Service.IEnumerable
{
    public class BulletinTemplateService : Service<BulletinTemplate>, IBulletinTemplateService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<BulletinTemplateMaster> _bulletinProcessRepository;
        private readonly IRepositoryAsync<BulletinTemplateDetail> _bulletinDetailRepository;
        private readonly IRepositoryAsync<BulletinTemplateBuyerInfo> _bulletinBuyerRepository;
        private readonly IRepositoryAsync<BulletinCalculation> _bulletinCalculationRepository;

        public BulletinTemplateService(
            IRepositoryAsync<BulletinTemplate> bulletinTemplateRepository
            , IRepositoryAsync<BulletinTemplateMaster> bulletinProcessRepository
            , IRepositoryAsync<BulletinTemplateDetail> bulletinDetailRepository
            , IRepositoryAsync<BulletinTemplateBuyerInfo> bulletinBuyerRepository
            , IRepositoryAsync<BulletinCalculation> bulletinCalculationRepository
            , IUnitOfWork unitOfWork
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository) : base(bulletinTemplateRepository, unitOfWork, pkGeneratorService)
        {
            _bulletinProcessRepository = bulletinProcessRepository;
            _bulletinDetailRepository = bulletinDetailRepository;
            _bulletinBuyerRepository = bulletinBuyerRepository;
            _bulletinCalculationRepository = bulletinCalculationRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region BulletinTemplate

        private void Check(BulletinTemplate entity)
        {
            CheckUniqueColumn(UniqueColumnName.BulletinName, entity.BulletinName, r => r.Id != entity.Id && r.BulletinName == entity.BulletinName && r.AlternativeName == entity.AlternativeName);
            //CheckUniqueColumn(UniqueColumnName.AlternativeName, entity.AlternativeName, r => r.Id != entity.Id && r.AlternativeName == entity.AlternativeName);
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(BulletinTemplate), out sID);
            return sID;
        }

        public override void Insert(BulletinTemplate entity)
        {
            try
            {

                object ob = base.Query(r => r.Id != entity.Id && r.BulletinName == entity.BulletinName && r.AlternativeName == entity.AlternativeName).Select().FirstOrDefault();

                if (ob != null)
                {
                    throw new CustomException("Same combination is exists.");
                }
                entity.Id = "B-" + GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(BulletinTemplate entity)
        {
            try
            {

                object ob = base.Query(r => r.Id != entity.Id && r.BulletinName == entity.BulletinName && r.AlternativeName == entity.AlternativeName).Select().FirstOrDefault();
                if (ob != null)
                {
                    throw new CustomException("Same combination is exists.");
                }
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> Query(string companyGroupId)
        {
            try
            {
                string sql = @"Select BT.*
                         ,PM.UserName ProductMaster, SG.UserName SizeGroup
						  ,Buyer=STUFF((select distinct ', '+B.UserName FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
										JOIN HKP.Buyer B ON B.Id=BTB.BuyerId
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			         	,BuyerItemRefNo=STUFF((select distinct ', '+BTB.BuyerStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
			        	,OwnStyleRefNo=STUFF((select distinct ', '+BTB.OwnStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

						,Process=STUFF((select distinct ', '+P.UserName FROM 
                                       [MST].[BulletinTemplateMaster] BTP 
									   join HKP.Process P ON P.Id=BTP.ProcessId
                                        WHERE BT.Id=BTP.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,PBCount=(Select Count (Id) From [TRN].[ProductionBulletinTemplate] Where BulletinTemplateId=BT.Id)			
                         FROM [MST].[BulletinTemplate] BT
                         LEFT JOIN MST.ProductMaster PM ON PM.Id=BT.ProductMasterId
                         LEFT JOIN HKP.SizeGroup SG ON SG.Id=BT.SizeGroupId WHERE BT.CompanyGroupId='" + companyGroupId + "' ORDER BY BT.AddedDate DESC";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public bool CheckUsing(object id)
        {
            try
            {
                var sql = @"IF EXISTS(SELECT 1 FROM( 
                          SELECT BTM.BulletinTemplateId AS CheckingColumn 
						  FROM  [MST].[BulletinTemplateDetail] BTD 
						  LEFT JOIN [MST].[BulletinTemplateMaster] BTM ON BTM.Id=BTD.BulletinTemplateMasterId
						  LEFT JOIN [MST].[BulletinTemplate] BT ON BT.Id=BTM.BulletinTemplateId
                          ) A WHERE CheckingColumn = '" + id + @"') SELECT 1 ELSE SELECT 0 RETURN";
                return Convert.ToBoolean(_bulletinDetailRepository.SqlQuery<int>(sql).Single());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteBulletin(string id)
        {
            string strSQL, strPSQL, strBSQL, strOSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //if (CheckUsing(id))
                //    throw new CustomException("First delete Operation!");

                strOSQL = "DELETE FROM  [MST].[BulletinTemplateDetail] WHERE BulletinTemplateMasterId IN (SELECT ID FROM  [MST].[BulletinTemplateMaster]  WHERE BulletinTemplateId='" + id + "')";
                strCSQL = "DELETE FROM dbo.BulletinCalculation WHERE BulletinTemplateMasterId IN (SELECT ID FROM  [MST].[BulletinTemplateMaster]  WHERE BulletinTemplateId='" + id + "')";
                strPSQL = "DELETE FROM [MST].[BulletinTemplateMaster] WHERE BulletinTemplateId='" + id + "'";
                strBSQL = "DELETE FROM [MST].[BulletinTemplateBuyerInfo] WHERE BulletinTemplateId='" + id + "'";
                strSQL = "DELETE FROM [MST].[BulletinTemplate] WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        #endregion

        #region BulletinProcess

        public IEnumerable<object> GetProcessQtyAndNoWSData(string processId, string productMasterId)
        {
            try
            {
                var sql = @"SELECT PM.TargetQty,PME.NoOfWorkStation FROM [MST].[ProductMaster] PM
                            LEFT JOIN [TRN].[ProductMasterEfficency] PME ON PME.ProductMasterId=PM.Id
                            WHERE PM.BaseProcessId='" + processId + @"' AND PM.Id='" + productMasterId + @"' AND PME.EfficencyName='Planning'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private string GetProcessPK()
        {
            //return GetAutoNumber(nameof(BulletinTemplateMaster), PKGeneratorEnum.Auto, null, DateTime.Now);

            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(BulletinTemplateMaster), out sID);
            return sID;

        }

        public void InsertOrUpdateProcess(BulletinTemplateMaster entity)
        {
            try
            {
                var checkProcess = _bulletinProcessRepository.Any(t => t.Id != entity.Id && t.BulletinTemplateId == entity.BulletinTemplateId && t.ProcessId == entity.ProcessId);
                if (!checkProcess)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = "BTM" + GetProcessPK();
                        AuditService.AddedLog(entity);
                        _bulletinProcessRepository.Insert(entity);
                        _unitOfWork.SaveChanges();
                    }
                    else
                    {
                        AuditService.UpdatedLog(entity);
                        _bulletinProcessRepository.Update(entity);
                        _unitOfWork.SaveChanges();
                    }
                }
                else
                {
                    throw new CustomException("Process should be unique.");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetBulletinProcess(string bulletinTemplateId)
        {
            try
            {
                string sql = @"SELECT P.UserName Process, BTM.* FROM [MST].[BulletinTemplateMaster] BTM
                             LEFT JOIN HKP.Process P ON P.Id=BTM.ProcessId
                             WHERE BTM.BulletinTemplateId='" + bulletinTemplateId + "' Order By P.[Sequence]";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public void DeleteProcess(string id)
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _bulletinProcessRepository.Find(id);
                var getOPdata = _bulletinDetailRepository.Query(t => t.BulletinTemplateMasterId == id).Select().FirstOrDefault();
                if (getOPdata != null)
                {
                    throw new CustomException("This process has operation, first delete it's operation.");
                }
                _bulletinProcessRepository.Delete(data);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion

        #region BulletinBuyer

        private string GetBuyerPK()
        {
            //return GetAutoNumber(nameof(BulletinTemplateBuyerInfo), PKGeneratorEnum.Auto, null, DateTime.Now);
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(BulletinTemplateBuyerInfo), out sID);
            return sID;
        }

        public void InsertOrUpdateBuyer(BulletinTemplateBuyerInfo entity)
        {
            try
            {
                //var data = _bulletinBuyerRepository.Query(t => t.Id != entity.Id && t.BulletinTemplateId == entity.BulletinTemplateId && t.BuyerId == entity.BuyerId).Select().FirstOrDefault();
                var checkBuyer = _bulletinBuyerRepository.Any(t => t.Id != entity.Id && t.BulletinTemplateId == entity.BulletinTemplateId && t.BuyerId == entity.BuyerId);
                if (!checkBuyer)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = GetBuyerPK();
                        AuditService.AddedLog(entity);
                        _bulletinBuyerRepository.Insert(entity);
                        _unitOfWork.SaveChanges();
                    }
                    else
                    {
                        AuditService.UpdatedLog(entity);
                        _bulletinBuyerRepository.Update(entity);
                        _unitOfWork.SaveChanges();
                    }
                }
                else
                {
                    throw new CustomException("Buyer is unique.");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetBulletinBuyer(string bulletinTemplateId)
        {
            try
            {
                string sql = @"SELECT B.UserName Buyer, BTB.* FROM [MST].[BulletinTemplateBuyerInfo] BTB
                             LEFT JOIN HKP.Buyer B ON B.Id=BTB.BuyerId
                             WHERE BTB.BulletinTemplateId='" + bulletinTemplateId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteBuyer(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _bulletinBuyerRepository.Find(id);
                _bulletinBuyerRepository.Delete(data);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        #endregion

        #region Operation
        public decimal GetAutoSequence()
        {
            try
            {
                return _bulletinDetailRepository.Query().Select().Max(t => t.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetOperationPK()
        {
            //return GetAutoNumber(nameof(BulletinTemplateDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(BulletinTemplateDetail), out sID);
            return sID;
        }

        public void GetAutoSequence(string bulletinTemplateMasterId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT ISNULL((MAX(Sequence)+1),0) Sequence FROM [MST].[BulletinTemplateDetail] Where BulletinTemplateMasterId='" + bulletinTemplateMasterId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CheckSequence(string id, decimal sequence, string bulletinTemplateMasterId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT Id,Sequence,BulletinTemplateMasterId FROM [MST].[BulletinTemplateDetail] Where Sequence='" + sequence + "' and Id<>'" + id + "' and BulletinTemplateMasterId='" + bulletinTemplateMasterId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetBulletinCalculation(string bulletinTemplateMasterId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT * FROM [dbo].[BulletinCalculation] Where BulletinTemplateMasterId='" + bulletinTemplateMasterId + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertOrUpdateOperation(IEnumerable<BulletinTemplateDetail> entities, string bulletinTemplateMasterId, BulletinCalculation bulletinCalculation)
        {
            try
            {
                DataSet dsSeq, dsSq, dsBC;
                GetAutoSequence(bulletinTemplateMasterId, out dsSeq);
                GetBulletinCalculation(bulletinTemplateMasterId, out dsBC);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Sequence"].ToString());
                if (seq != 0)
                {
                    seq--;
                }
                EvaluateSPI(entities);

                foreach (var item in entities)
                {

                    // CheckSequence(item.Id,item.Sequence, bulletinTemplateMasterId, out dsSq);
                    //if (dsSq.Tables[0].Rows.Count>0)
                    //{
                    //    if (item.Sequence== Convert.ToDecimal(dsSq.Tables[0].Rows[0]["Sequence"].ToString()))
                    //    {
                    //        throw new Exception("Sequence "+ item.Sequence + " is already exists.");
                    //    }
                    //}

                    if (string.IsNullOrEmpty(item.Id))
                    {
                        seq++;
                        item.Id = "BTD" + GetOperationPK();
                        item.Sequence = seq;
                        AuditService.AddedLog(item);
                        _bulletinDetailRepository.Insert(item);
                        _unitOfWork.SaveChanges();
                    }
                    else
                    {
                        AuditService.UpdatedLog(item);
                        _bulletinDetailRepository.Update(item);
                        _unitOfWork.SaveChanges();
                    }
                }

                if (bulletinCalculation != null)
                {
                    if (dsBC.Tables[0].Rows.Count == 0)
                    {
                        AuditService.AddedLog(bulletinCalculation);
                        if (bulletinCalculation.OrganizationEfficiency== "Infinity")
                        {
                            bulletinCalculation.OrganizationEfficiency = "0"; 
                        }
                        if (bulletinCalculation.PitchTime == "Infinity")
                        {
                            bulletinCalculation.PitchTime = "0"; 
                        }
                        _bulletinCalculationRepository.Insert(bulletinCalculation);
                        _unitOfWork.SaveChanges();
                    }
                    else
                    {
                        bulletinCalculation.Id = Convert.ToInt32(dsBC.Tables[0].Rows[0]["Id"].ToString());
                        if (bulletinCalculation.OrganizationEfficiency == "Infinity")
                        {
                            bulletinCalculation.OrganizationEfficiency = "0";
                        }
                        if (bulletinCalculation.PitchTime == "Infinity")
                        {
                            bulletinCalculation.PitchTime = "0";
                        }
                        AuditService.UpdatedLog(bulletinCalculation);
                        _bulletinCalculationRepository.Update(bulletinCalculation);
                        _unitOfWork.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private void EvaluateSPI(IEnumerable<BulletinTemplateDetail> ItemDetail)
        {
            string StitchCodeIds = "''"; string SPIs = "NULL";
            foreach (var item in ItemDetail)
            {
                if (string.IsNullOrEmpty(item.StitchCodeId) == false)
                {
                    StitchCodeIds += ",'" + item.StitchCodeId + "'";
                    SPIs += "," + item.SPI + "";

                }

            }

            DataTable dtFormula = _sqlRepository.GetDataTable(@" SELECT f.*,SC.Needle,sc.Bobbin,sc.Looper FROM   [dbo].[SPIFormula]  F
                                    inner join hkp.StitchCode SC ON SC.id=f.StitchCodeId  WHERE f.StitchCodeId in (" + StitchCodeIds + ") and f.SPI IN (" + SPIs + ")");

            string Formula;
            double SPI, FabricWht, SPIConsumption, NeedleConsumption, BobbinConsumption, LooperConsumption;
            decimal Consumption;
            foreach (var item in ItemDetail)
            {
                dtFormula.DefaultView.RowFilter = "StitchCodeId='" + item.StitchCodeId + "' AND SPI=" + item.SPI;
                if (dtFormula.DefaultView.Count > 0)
                {
                    Formula = bplib.clsWebLib.GetBoolData(dtFormula.DefaultView[0]["isFormula"].ToString()) == true ? dtFormula.DefaultView[0]["Formula"].ToString() : dtFormula.DefaultView[0]["FixedValue"].ToString();
                    SPI = item.SPI;
                    FabricWht = (double)item.FabricWidth;

                    Library.Service.Helpers.ThreadConsumption threadConsumption = new Helpers.ThreadConsumption(
                        new Helpers.ThreadConsumption.FKeys { Key = Helpers.ThreadConsumption.FxKeys.SPI, Value = SPI },
                        new Helpers.ThreadConsumption.FKeys { Key = Helpers.ThreadConsumption.FxKeys.FabWht, Value = FabricWht }
                        );

                    SPIConsumption = threadConsumption.ExecuteFunction(Formula);
                    Consumption = (item.OperationLength * (decimal)SPIConsumption * item.NoOfStitch) / 100;
                    //NeedleConsumption = threadConsumption.ExecuteFunction(Formula,clsStaticInfo.dbl(dtFormula.DefaultView[0]["Needle"].ToString()));
                    //BobbinConsumption = threadConsumption.ExecuteFunction(Formula, clsStaticInfo.dbl(dtFormula.DefaultView[0]["Bobbin"].ToString()));
                    //LooperConsumption= threadConsumption.ExecuteFunction(Formula, clsStaticInfo.dbl(dtFormula.DefaultView[0]["Looper"].ToString()));

                    NeedleConsumption = threadConsumption.ExecuteFunction(Consumption.ToString(), clsStaticInfo.dbl(dtFormula.DefaultView[0]["Needle"].ToString()));
                    BobbinConsumption = threadConsumption.ExecuteFunction(Consumption.ToString(), clsStaticInfo.dbl(dtFormula.DefaultView[0]["Bobbin"].ToString()));
                    LooperConsumption = threadConsumption.ExecuteFunction(Consumption.ToString(), clsStaticInfo.dbl(dtFormula.DefaultView[0]["Looper"].ToString()));



                    item.SPIConsumption = (decimal)SPIConsumption;
                    item.Consumption = (decimal)Consumption;
                    item.NeedleConsumption = (decimal)NeedleConsumption;
                    item.BobbinConsumption = (decimal)BobbinConsumption;
                    item.LooperConsumption = (decimal)LooperConsumption;
                    //item.PerOperationConsumption = (decimal)PerOperationConsumption;

                }
            }

        }

        public void UpdateMachine(BulletinTemplateDetail entity)
        {
            try
            {
                var dblist = _bulletinDetailRepository.Find(entity.Id);

                dblist.MachineVarientId = entity.MachineVarientId;
                dblist.SkillMasterId = entity.SkillMasterId;

                AuditService.UpdatedLog(dblist);
                _bulletinDetailRepository.Update(dblist);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateSequence(BulletinTemplateDetail entity)
        {
            //DataSet  dsSq;
            try
            {
                //CheckSequence(entity.Sequence, out dsSq);
                //if (dsSq.Tables[0].Rows.Count > 0)
                //{
                //    if (entity.Sequence == Convert.ToDecimal(dsSq.Tables[0].Rows[0]["Sequence"].ToString()))
                //    {
                //        throw new Exception("Sequence " + entity.Sequence + " is already exists.");
                //    }
                //}

                var dblist = _bulletinDetailRepository.Find(entity.Id);

                dblist.Sequence = entity.Sequence;

                AuditService.UpdatedLog(dblist);
                _bulletinDetailRepository.Update(dblist);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetBulletinOperation(string bulletinTemplateMasterId)
        {
            try
            {
                string sql = @"SELECT BTD.Id,BTD.BulletinTemplateMasterId,BTD.Sequence,BTD.OperationVariationId,BTD.OperationGroup,BTD.SkillMasterId,BTD.MachineVarientId,BTD.FGZoneId,BTD.FGComponentId
                            ,CONVERT(NUMERIC(10,2),BTD.AdditionalSPT) AdditionalSPT, CONVERT(NUMERIC(10,2),BTD.TotalSPT) TotalSPT, CONVERT(NUMERIC(10,2),BTD.AllotedWorkstation) AllotedWorkstation
                            , CONVERT(NUMERIC(10,2),BTD.AllotedManpower) AllotedManpower, BTD.AttachmentId,BTD.GaugeFolderId,BTD.OperationConsumptionId,BTD.OperationTypeId,CONVERT(NUMERIC(10,2),BTD.Frequency) Frequency
                            ,BTD.Remark,BTD.OperationCategoryId,BTD.QualityLevel,CONVERT(NUMERIC(10,2),BTD.AvgAllotedTime) AvgAllotedTime,CONVERT(NUMERIC(10,0),BTD.OperationTargetPerHr) OperationTargetPerHr
                            ,CONVERT(NUMERIC(10,0),BTD.RequiredManPower) RequiredManPower
                            ,OV.Code OperationCode, OV.UserName OperationVariation, FZ.UserName FGZone, FC.UserName FGComponent, A.UserName Attachment,
                             GF.UserName GaugeFolder, OC.UserName OperationConsumption, OT.UserName OperationType, OV.OperationId, MMA.StandardName MachineName
                            ,0 AvgAllotedTime, OperationSPT=BTD.TotalSPT-BTD.AdditionalSPT, MM.UserName MaterialMaster, 0 IsMaxAllottedTime 
                            , OM.UserName AS SkillName,OPP.BasicProcessTime,OPP.AssociateProcessTime,OPP.PersonalAllowance,OV.MachineAllowance,OPP.Frequency,OPP.SPI OperationSPI,OV.TotalSAM, OV.AdditionalSAMSymbol,OV.SubOperationSAM,OV.AdditionalSAM
                            ,BTD.SPI,BTD.NoOfStitch,BTD.OperationLength,BTD.StitchCodeId,BTD.FabricWidth,OV.AdditionalAllowance,ISNULL(OV.VASSAMSOURCE,'') VASSAMSOURCE
                             ,BTD.NeedleDescription,BTD.NeedleMaterialMasterId,BTD.NeedleArticleId	
                            ,BTD.BobbinDescription,BTD.BobbinMaterialMasterId,BTD.BobbinArticleId
                            ,BTD.LooperDescription,BTD.LooperMaterialMasterId,BTD.LooperArticleId	
                            ,BTD.SPIConsumption,BTD.NeedleConsumption,BTD.BobbinConsumption,BTD.LooperConsumption,BTD.Consumption,0 DelFlag
                            ,CONVERT(NUMERIC(10,2),BTD.AdditionalWorkstation) AdditionalWorkstation, CONVERT(NUMERIC(10,2),BTD.AdditionalManpower) AdditionalManpower,BTD.AreaCode
                             FROM [MST].[BulletinTemplateDetail] BTD
                             LEFT JOIN [MST].[OperationVariation] OV ON OV.Id=BTD.OperationVariationId
                             LEFT JOIN (SELECT OP.Id,ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                                     ,ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance, ISNULL(OP.MachineAllowance, 0) AS MachineAllowance
                                     ,OP.Frequency, OP.SPI FROM [MST].[Operation] OP) OPP ON OPP.Id =OV.OperationId
                             LEFT JOIN HKP.FGZone FZ ON FZ.Id=BTD.FGZoneId
                             LEFT JOIN HKP.FGComponent FC ON FC.Id=BTD.FGComponentId
                             LEFT JOIN HKP.Attachment A ON A.Id=BTD.AttachmentId
                             LEFT JOIN HKP.GaugeFolder GF ON GF.Id=BTD.GaugeFolderId
                             LEFT JOIN HKP.OperationConsumption OC ON OC.Id=BTD.OperationConsumptionId
                             LEFT JOIN HKP.OperationType OT ON OT.Id=BTD.OperationTypeId
                             LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = BTD.MachineVarientId
                             LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=MMA.MaterialMasterId
							 LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = BTD.SkillMasterId
                             WHERE BTD.BulletinTemplateMasterId='" + bulletinTemplateMasterId + "' ORDER BY BTD.Sequence ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetOperationData(string companyGroupId, string processId, string bulletinTemplateId, string productMasterId)
        {
            try
            {
                var sql = @"SELECT CONVERT (bit,0) Active
                           	,OV.Id OperationVariationId
                           	,OV.Code OperationCode
                           	,OV.[Sequence]
                           	,A.Id MachineVarientId
							,MM.UserName MaterialMaster
                           	,A.StandardName Article
							,OV.OperationMasterId SkillMasterId
                           	,OM.UserName SkillName
                           	,OV.UserName OperationVariation
                           	,OV.SubOperationSAM
                           	,OV.AdditionalSAM
                           	,OV.SPI,ISNULL(OV.VASSAMSOURCE,'') VASSAMSOURCE
                           	,ISNULL(OV.VASFINALSAM,OV.TotalSAM) TtalSAM
							,TotalSAM=CASE WHEN ISNULL(OV.VASSAMSOURCE,'')='' THEN OV.TotalSAM ELSE OV.VASFINALSAM END
                           	,OV.Frequency
                            ,OT.Id OperationTypeId
                            ,OV.AdditionalSAMSymbol
                            ,OV.OperationId
                            ,OCT.Id OperationCategoryId
							,OCT.UserName OperationCategory
                            ,SC.Id StitchCodeId ,SC.UserName StitchCode,OperationLength=ISNULL(O.OperationLength,0)* 2.54,OV.AreaCode
                           FROM [MST].[OperationVariation] OV
                           LEFT JOIN [MST].[MaterialMasterArticle] A ON A.Id = OV.ArticleId
                           --LEFT JOIN [HKP].[Skill] S ON S.Id = OV.SkillId
                           LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=A.MaterialMasterId --AND MM.SkillId=S.Id
                           LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                           LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OV.OperationMasterId
                           LEFT JOIN [HKP].[OperationType] OT ON OT.Id = O.OperationTypeId
                           LEFT JOIN [HKP].[OperationCategory] OCT ON OCT.Id = O.OperationCategoryId
                           LEFT JOIN [HKP].[StitchCode] SC ON SC.Id = A.StitchCodeId
						   INNER JOIN (Select * from [MST].[OperationProcess] WHERE ProcessId='" + processId + @"')OP ON OP.OperationId=OV.OperationId
                           WHERE OV.CompanyGroupId = '" + companyGroupId + @"' AND OV.Id IN(Select OperationVariationId FROM dbo.OperationVariationProductMaster Where ProductMasterId='" + productMasterId + @"')
                           --AND OV.Id NOT IN (SELECT BTD.OperationVariationId FROM [MST].[BulletinTemplateDetail] BTD
					       --LEFT JOIN [MST].[BulletinTemplateMaster] BTM ON BTM.Id=BTD.BulletinTemplateMasterId
					       --Where BTM.BulletinTemplateId='" + bulletinTemplateId + @"') 
                            ORDER BY OV.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public void InsertOperation(IdentityParameter para, string Code, string processId, string bulletinTemplateMasterId)
        {
            try
            {
                DataSet ds = GetOperationDataByCode(para.CompanyGroupId, Code, processId, bulletinTemplateMasterId);
                SaveBulletinDetailData(ds, para, bulletinTemplateMasterId);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void ReplaceOperation(IdentityParameter para, string Code, decimal Sequence, string processId, string bulletinTemplateMasterId)
        {
            try
            {
                DataSet ds = GetOperationDataByCode(para.CompanyGroupId, Code, processId, bulletinTemplateMasterId);
                SaveReplacedBulletinDetailData(ds, para, bulletinTemplateMasterId, Sequence);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private void SaveReplacedBulletinDetailData(DataSet dataSet, IdentityParameter para, string BulletinTemplateMasterId, decimal Sequence)
        {
            ConnectionManager.DAL.ConManager objCon;
            var id = "BTD" + GetOperationPK();
            string sql = "SELECT * FROM [MST].[BulletinTemplateDetail] WHERE Id=''";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out DataSet dsOperation, false, "1");
            int count = 0;


            if (dataSet.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    count++;

                    DataRow dr = dsOperation.Tables[0].NewRow();

                    dr["Id"] = id + "-" + count;
                    dr["BulletinTemplateMasterId"] = BulletinTemplateMasterId;
                    dr["Sequence"] = Sequence;
                    dr["OperationVariationId"] = dataSet.Tables[0].Rows[i]["OperationVariationId"];
                    dr["OperationGroup"] = null;
                    dr["SkillMasterId"] = dataSet.Tables[0].Rows[i]["SkillMasterId"];
                    dr["MachineVarientId"] = dataSet.Tables[0].Rows[i]["MachineVarientId"];
                    dr["FGZoneId"] = null;
                    dr["FGComponentId"] = null;
                    dr["AdditionalSPT"] = dataSet.Tables[0].Rows[i]["AdditionalSAM"];
                    dr["TotalSPT"] = dataSet.Tables[0].Rows[i]["TotalSAM"];
                    dr["AllotedWorkstation"] = 0;
                    dr["AllotedManpower"] = 0;
                    dr["AvgAllotedTime"] = 0;
                    dr["AttachmentId"] = null;
                    dr["GaugeFolderId"] = null;
                    dr["OperationConsumptionId"] = null;
                    dr["OperationTypeId"] = dataSet.Tables[0].Rows[i]["OperationTypeId"];
                    dr["Frequency"] = dataSet.Tables[0].Rows[i]["Frequency"];
                    dr["Remark"] = null;
                    dr["OperationCategoryId"] = dataSet.Tables[0].Rows[i]["OperationCategoryId"];
                    dr["QualityLevel"] = null;
                    dr["OperationTargetPerHr"] = 0;
                    dr["RequiredManPower"] = 0;

                    dr["SPI"] = dataSet.Tables[0].Rows[i]["SPI"];
                    dr["NoOfStitch"] = 0;
                    dr["OperationLength"] = dataSet.Tables[0].Rows[i]["OperationLength"];
                    dr["StitchCodeId"] = dataSet.Tables[0].Rows[i]["StitchCodeId"];
                    dr["FabricWidth"] = 0;
                    dr["NeedleDescription"] = null;
                    dr["NeedleMaterialMasterId"] = null;
                    dr["NeedleArticleId"] = null;
                    dr["BobbinMaterialMasterId"] = null;
                    dr["BobbinArticleId"] = null;
                    dr["LooperDescription"] = null;
                    dr["LooperMaterialMasterId"] = null;
                    dr["LooperArticleId"] = null;
                    dr["SPIConsumption"] = 0;
                    dr["NeedleConsumption"] = 0;
                    dr["BobbinConsumption"] = 0;
                    dr["LooperConsumption"] = 0;
                    dr["Consumption"] = 0;
                    dr["WastagePercentage"] = 0;
                    dr["ExtraOrderPercentage"] = 0;

                    dr["AddedBy"] = para.AddedBy;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = para.AddedFromIP;


                    dsOperation.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsOperation);
            }
            else
            {
                throw new Exception("Wrong Operation Code !!!.");
            }

        }

        private DataSet GetOperationDataByCode(string companyGroupId, string Code, string processId, string bulletinTemplateMasterId)
        {
            try
            {
                GridParameter parameters;
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT CONVERT (bit,0) Active
                           	,OV.Id OperationVariationId
                           	,OV.Code OperationCode
                           	,OV.[Sequence]
                           	,A.Id MachineVarientId
							,MM.UserName MaterialMaster
                           	,A.StandardName Article
							,OM.Id SkillMasterId
                           	,OM.UserName SkillName
                           	,OV.UserName OperationVariation
                           	,OV.SubOperationSAM
                           	,OV.AdditionalSAM
                           	,OV.SPI,OV.VASSAMSOURCE
                           	,ISNULL(OV.VASFINALSAM,OV.TotalSAM) TtalSAM
							,TotalSAM=CASE WHEN ISNULL(OV.VASSAMSOURCE,'')='' THEN OV.TotalSAM ELSE OV.VASFINALSAM END
                           	,OV.Frequency
                            ,OT.Id OperationTypeId
                            ,OV.AdditionalSAMSymbol
                            ,OV.OperationId
                            ,OCT.Id OperationCategoryId
							,OCT.UserName OperationCategory
                            ,SC.Id StitchCodeId ,SC.UserName StitchCode,O.OperationLength
                           FROM [MST].[OperationVariation] OV
                           LEFT JOIN [MST].[MaterialMasterArticle] A ON A.Id = OV.ArticleId
                           LEFT JOIN [MST].[OperationMaster] OM ON OM.Id = OV.OperationMasterId
                           LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=A.MaterialMasterId --AND MM.SkillId=S.Id
                           LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                           LEFT JOIN [HKP].[OperationType] OT ON OT.Id = O.OperationTypeId
                           LEFT JOIN [HKP].[OperationCategory] OCT ON OCT.Id = O.OperationCategoryId
                           LEFT JOIN [HKP].[StitchCode] SC ON SC.Id = A.StitchCodeId
						   INNER JOIN (Select * from [MST].[OperationProcess] WHERE ProcessId='" + processId + @"')OP ON OP.OperationId=OV.OperationId
                           WHERE OV.CompanyGroupId = '" + companyGroupId + @"' AND OV.Code IN (" + Code + @") "
                };


                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private void SaveBulletinDetailData(DataSet dataSet, IdentityParameter para, string BulletinTemplateMasterId)
        {
            ConnectionManager.DAL.ConManager objCon;
            var id = "BTD" + GetOperationPK();
            string sql = "SELECT * FROM [MST].[BulletinTemplateDetail] WHERE Id=''";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out DataSet dsOperation, false, "1");
            int count = 0;

            DataSet dsSeq;
            GetAutoSequence(BulletinTemplateMasterId, out dsSeq);
            decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Sequence"].ToString());
            if (seq > 0)
            {
                seq--;
            }
            else
            {
                seq++;
            }

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    count++;
                    seq++;
                    var sq = seq++;
                    DataRow dr = dsOperation.Tables[0].NewRow();

                    dr["Id"] = id + "-" + count;
                    dr["BulletinTemplateMasterId"] = BulletinTemplateMasterId;
                    dr["Sequence"] = sq;
                    dr["OperationVariationId"] = dataSet.Tables[0].Rows[i]["OperationVariationId"];
                    dr["OperationGroup"] = null;
                    dr["SkillMasterId"] = dataSet.Tables[0].Rows[i]["SkillMasterId"];
                    dr["MachineVarientId"] = dataSet.Tables[0].Rows[i]["MachineVarientId"];
                    dr["FGZoneId"] = null;
                    dr["FGComponentId"] = null;
                    dr["AdditionalSPT"] = dataSet.Tables[0].Rows[i]["AdditionalSAM"];
                    dr["TotalSPT"] = dataSet.Tables[0].Rows[i]["TotalSAM"];
                    dr["AllotedWorkstation"] = 0;
                    dr["AllotedManpower"] = 0;
                    dr["AvgAllotedTime"] = 0;
                    dr["AttachmentId"] = null;
                    dr["GaugeFolderId"] = null;
                    dr["OperationConsumptionId"] = null;
                    dr["OperationTypeId"] = dataSet.Tables[0].Rows[i]["OperationTypeId"];
                    dr["Frequency"] = dataSet.Tables[0].Rows[i]["Frequency"];
                    dr["Remark"] = null;
                    dr["OperationCategoryId"] = dataSet.Tables[0].Rows[i]["OperationCategoryId"];
                    dr["QualityLevel"] = null;
                    dr["OperationTargetPerHr"] = 0;
                    dr["RequiredManPower"] = 0;

                    dr["SPI"] = dataSet.Tables[0].Rows[i]["SPI"];
                    dr["NoOfStitch"] = 0;
                    dr["OperationLength"] = dataSet.Tables[0].Rows[i]["OperationLength"];
                    dr["StitchCodeId"] = dataSet.Tables[0].Rows[i]["StitchCodeId"];
                    dr["FabricWidth"] = 0;
                    dr["NeedleDescription"] = null;
                    dr["NeedleMaterialMasterId"] = null;
                    dr["NeedleArticleId"] = null;
                    dr["BobbinMaterialMasterId"] = null;
                    dr["BobbinArticleId"] = null;
                    dr["LooperDescription"] = null;
                    dr["LooperMaterialMasterId"] = null;
                    dr["LooperArticleId"] = null;
                    dr["SPIConsumption"] = 0;
                    dr["NeedleConsumption"] = 0;
                    dr["BobbinConsumption"] = 0;
                    dr["LooperConsumption"] = 0;
                    dr["Consumption"] = 0;
                    dr["WastagePercentage"] = 0;
                    dr["ExtraOrderPercentage"] = 0;

                    dr["AddedBy"] = para.AddedBy;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = para.AddedFromIP;


                    dsOperation.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsOperation);

            }
            else
            {
                throw new Exception("Wrong Operation Code !!!.");
            }

        }

        public GridModel GetCbo(string processId)
        {
            try
            {
                var sql = @"SELECT ART.Id [Value], ART.StandardName [Text], OV.TotalSAM OperationSPT FROM MST.OperationVariation OV
                            LEFT JOIN [MST].[MaterialMasterArticle] ART ON ART.Id=OV.ArticleId
                            LEFT JOIN [MST].[OperationProcess] OP ON OP.OperationId=OV.OperationId
                            WHERE OP.ProcessId='" + processId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Addresse.ToString()));
            }
        }

        public void DeleteOperation(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = _bulletinDetailRepository.Find(id);
                _bulletinDetailRepository.Delete(data);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }



        #endregion

        #region Copy


        public void Copy(BulletinTemplate entity)
        {
            try
            {
                SaveBulletinData(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private DataSet GetBulletinTemplateBuyer(string bulletinTemplateId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [MST].[BulletinTemplateBuyerInfo] WHERE BulletinTemplateId='" + bulletinTemplateId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private DataSet GetBulletinTemplateMaster(string bulletinTemplateId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [MST].[BulletinTemplateMaster]  WHERE BulletinTemplateId='" + bulletinTemplateId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private void SaveBulletinData(BulletinTemplate data)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                DataSet dsBuyer, dsProcess, dsOperation;
                string sql = "SELECT * FROM [MST].[BulletinTemplate] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "B-" + GetPK();
                    dr["CompanyGroupId"] = data.CompanyGroupId;
                    dr["BulletinName"] = data.BulletinName + "-" + "Copy";
                    dr["AlternativeName"] = data.AlternativeName;
                    dr["ByWhom"] = data.ByWhom;
                    dr["ProductMasterId"] = data.ProductMasterId;
                    dr["SizeGroupId"] = data.SizeGroupId;
                    dr["PicFileName"] = data.PicFileName;
                    dr["AddedBy"] = data.AddedBy;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = data.AddedFromIP;
                    dr["UpdatedBy"] = data.UpdatedBy;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = data.UpdatedFromIP;
                    dsMaster.Tables[0].Rows.Add(dr);
                }

                string CopiedBulletinId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                string NewBulletinId = dsMaster.Tables[0].Rows[1]["Id"].ToString();
                string PicFileName = dsMaster.Tables[0].Rows[1]["PicFileName"].ToString();

                DataSet dataSetBuyer = GetBulletinTemplateBuyer(CopiedBulletinId);
                SaveBulletinTemplateBuyerData(dataSetBuyer, NewBulletinId, CopiedBulletinId, out dsBuyer);



                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsBuyer);


                DataSet dataSetProcess = GetBulletinTemplateMaster(CopiedBulletinId);
                SaveBulletinMasterData(dataSetProcess, NewBulletinId, CopiedBulletinId, out dsProcess, out dsOperation);

                MoveImage(CopiedBulletinId, PicFileName, NewBulletinId);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public static void MoveImage(string fromName, string toName, string NewBulletinId)
        {
            var Fromdirectory = ResourcesPathReader.GetBulletinImagePath();
            var Todirectory = ResourcesPathReader.GetBulletinImagePath();
            if (!string.IsNullOrEmpty(fromName))
            {
                string path = Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName));
                //var path = Path.Combine(Fromdirectory, fromName);
                if (File.Exists(path))
                {
                    //File.Copy(Path.Combine(Fromdirectory, fromName), Path.Combine(Todirectory, NewBulletinId), true);
                    File.Copy(Path.Combine(Fromdirectory, fromName + Path.GetExtension(toName)), Path.Combine(Todirectory, NewBulletinId + Path.GetExtension(toName)), true);
                }
            }
        }
        private void SaveBulletinTemplateBuyerData(DataSet dataSet, string NewBulletinId, string OldBulletinId, out DataSet dsBuyer)
        {
            ConnectionManager.DAL.ConManager objCon;
            dsBuyer = null;
            string sql = "SELECT * FROM [MST].[BulletinTemplateBuyerInfo] WHERE Id=''";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsBuyer, false, "1");

            if (dataSet.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    var id = GetBuyerPK();

                    DataRow dr = dsBuyer.Tables[0].NewRow();

                    dr["Id"] = id;
                    dr["BulletinTemplateId"] = NewBulletinId;
                    dr["BuyerId"] = dataSet.Tables[0].Rows[i]["BuyerId"];
                    dr["BuyerStyleRefNo"] = dataSet.Tables[0].Rows[i]["BuyerStyleRefNo"];
                    dr["OwnStyleRefNo"] = dataSet.Tables[0].Rows[i]["OwnStyleRefNo"];
                    dr["AddedBy"] = dataSet.Tables[0].Rows[i]["AddedBy"];
                    dr["AddedDate"] = dataSet.Tables[0].Rows[i]["AddedDate"];
                    dr["AddedFromIP"] = dataSet.Tables[0].Rows[i]["AddedFromIP"];
                    dr["UpdatedBy"] = dataSet.Tables[0].Rows[i]["UpdatedBy"];
                    dr["UpdatedDate"] = dataSet.Tables[0].Rows[i]["UpdatedDate"];
                    dr["UpdatedFromIP"] = dataSet.Tables[0].Rows[i]["UpdatedFromIP"];

                    dsBuyer.Tables[0].Rows.Add(dr);
                }
            }
        }

        private void SaveBulletinMasterData(DataSet dataSet, string NewBulletinId, string OldBulletinId, out DataSet dsProcessMaster, out DataSet dsOperation)
        {
            dsProcessMaster = null;
            dsOperation = null;
            ConnectionManager.DAL.ConManager objCon;
            string BulletinTemplateMasterId = null;
            string sql = "SELECT * FROM [MST].[BulletinTemplateMaster] WHERE Id=''";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsProcessMaster, false, "1");
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    var id = GetProcessPK();

                    DataRow dr = dsProcessMaster.Tables[0].NewRow();

                    BulletinTemplateMasterId = id;
                    dr["Id"] = id;
                    dr["BulletinTemplateId"] = NewBulletinId;
                    dr["ProcessId"] = dataSet.Tables[0].Rows[i]["ProcessId"];
                    dr["RequiredStdTarget"] = dataSet.Tables[0].Rows[i]["RequiredStdTarget"];
                    dr["MaxNoOfWS"] = dataSet.Tables[0].Rows[i]["MaxNoOfWS"];
                    dr["PlannedHoursPerDay"] = dataSet.Tables[0].Rows[i]["PlannedHoursPerDay"];
                    dr["BottleNeckPercentage"] = dataSet.Tables[0].Rows[i]["BottleNeckPercentage"];
                    dr["AddedBy"] = dataSet.Tables[0].Rows[i]["AddedBy"];
                    dr["AddedDate"] = dataSet.Tables[0].Rows[i]["AddedDate"];
                    dr["AddedFromIP"] = dataSet.Tables[0].Rows[i]["AddedFromIP"];

                    dr["UpdatedBy"] = dataSet.Tables[0].Rows[i]["UpdatedBy"];
                    dr["UpdatedDate"] = dataSet.Tables[0].Rows[i]["UpdatedDate"];
                    dr["UpdatedFromIP"] = dataSet.Tables[0].Rows[i]["UpdatedFromIP"];

                    dsProcessMaster.Tables[0].Rows.Add(dr);

                    DataSet dsDetails = GetBulletinTemplateDetailData(dataSet.Tables[0].Rows[i]["Id"].ToString());
                    if (dsDetails.Tables[0].Rows.Count > 0)
                    {
                        SaveBulletinDetailData(dsDetails, BulletinTemplateMasterId, out dsOperation);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsProcessMaster, dsOperation);
                }
            }




        }

        private DataSet GetBulletinTemplateDetailData(string bulletinTemplateMasterId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [MST].[BulletinTemplateDetail] WHERE BulletinTemplateMasterId='" + bulletinTemplateMasterId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        private void SaveBulletinDetailData(DataSet dataSet, string BulletinTemplateMasterId, out DataSet dsOperation)
        {
            ConnectionManager.DAL.ConManager objCon;
            dsOperation = null;
            var id = "BTD" + GetOperationPK();
            string sql = "SELECT * FROM [MST].[BulletinTemplateDetail] WHERE Id=''";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsOperation, false, "1");
            int count = 0;
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    count++;

                    DataRow dr = dsOperation.Tables[0].NewRow();

                    dr["Id"] = id + "-" + count;
                    dr["BulletinTemplateMasterId"] = BulletinTemplateMasterId;
                    dr["Sequence"] = dataSet.Tables[0].Rows[i]["Sequence"];
                    dr["OperationVariationId"] = dataSet.Tables[0].Rows[i]["OperationVariationId"];
                    dr["OperationGroup"] = dataSet.Tables[0].Rows[i]["OperationGroup"];
                    dr["SkillMasterId"] = dataSet.Tables[0].Rows[i]["SkillMasterId"];
                    dr["MachineVarientId"] = dataSet.Tables[0].Rows[i]["MachineVarientId"];
                    dr["FGZoneId"] = dataSet.Tables[0].Rows[i]["FGZoneId"];
                    dr["FGComponentId"] = dataSet.Tables[0].Rows[i]["FGComponentId"];
                    dr["AdditionalSPT"] = dataSet.Tables[0].Rows[i]["AdditionalSPT"];
                    dr["TotalSPT"] = dataSet.Tables[0].Rows[i]["TotalSPT"];
                    dr["AllotedWorkstation"] = dataSet.Tables[0].Rows[i]["AllotedWorkstation"];
                    dr["AllotedManpower"] = dataSet.Tables[0].Rows[i]["AllotedManpower"];
                    dr["AvgAllotedTime"] = dataSet.Tables[0].Rows[i]["AvgAllotedTime"];
                    dr["AttachmentId"] = dataSet.Tables[0].Rows[i]["AttachmentId"];
                    dr["GaugeFolderId"] = dataSet.Tables[0].Rows[i]["GaugeFolderId"];
                    dr["OperationConsumptionId"] = dataSet.Tables[0].Rows[i]["OperationConsumptionId"];
                    dr["OperationTypeId"] = dataSet.Tables[0].Rows[i]["OperationTypeId"];
                    dr["Frequency"] = dataSet.Tables[0].Rows[i]["Frequency"];
                    dr["Remark"] = dataSet.Tables[0].Rows[i]["Remark"];
                    dr["OperationCategoryId"] = dataSet.Tables[0].Rows[i]["OperationCategoryId"];
                    dr["QualityLevel"] = dataSet.Tables[0].Rows[i]["QualityLevel"];
                    dr["OperationTargetPerHr"] = dataSet.Tables[0].Rows[i]["OperationTargetPerHr"];
                    dr["RequiredManPower"] = dataSet.Tables[0].Rows[i]["RequiredManPower"];

                    dr["SPI"] = dataSet.Tables[0].Rows[i]["SPI"];
                    dr["NoOfStitch"] = dataSet.Tables[0].Rows[i]["NoOfStitch"];
                    dr["OperationLength"] = dataSet.Tables[0].Rows[i]["OperationLength"];
                    dr["StitchCodeId"] = dataSet.Tables[0].Rows[i]["StitchCodeId"];
                    dr["FabricWidth"] = dataSet.Tables[0].Rows[i]["FabricWidth"];
                    dr["NeedleDescription"] = dataSet.Tables[0].Rows[i]["NeedleDescription"];
                    dr["NeedleMaterialMasterId"] = dataSet.Tables[0].Rows[i]["NeedleMaterialMasterId"];
                    dr["NeedleArticleId"] = dataSet.Tables[0].Rows[i]["NeedleArticleId"];
                    dr["BobbinMaterialMasterId"] = dataSet.Tables[0].Rows[i]["BobbinMaterialMasterId"];
                    dr["BobbinArticleId"] = dataSet.Tables[0].Rows[i]["BobbinArticleId"];
                    dr["LooperDescription"] = dataSet.Tables[0].Rows[i]["LooperDescription"];
                    dr["LooperMaterialMasterId"] = dataSet.Tables[0].Rows[i]["LooperMaterialMasterId"];
                    dr["LooperArticleId"] = dataSet.Tables[0].Rows[i]["LooperArticleId"];
                    dr["SPIConsumption"] = dataSet.Tables[0].Rows[i]["SPIConsumption"];
                    dr["NeedleConsumption"] = dataSet.Tables[0].Rows[i]["NeedleConsumption"];
                    dr["BobbinConsumption"] = dataSet.Tables[0].Rows[i]["BobbinConsumption"];
                    dr["LooperConsumption"] = dataSet.Tables[0].Rows[i]["LooperConsumption"];
                    dr["Consumption"] = dataSet.Tables[0].Rows[i]["Consumption"];
                    dr["WastagePercentage"] = dataSet.Tables[0].Rows[i]["WastagePercentage"];
                    dr["ExtraOrderPercentage"] = dataSet.Tables[0].Rows[i]["ExtraOrderPercentage"];

                    dr["AddedBy"] = dataSet.Tables[0].Rows[i]["AddedBy"];
                    dr["AddedDate"] = dataSet.Tables[0].Rows[i]["AddedDate"];
                    dr["AddedFromIP"] = dataSet.Tables[0].Rows[i]["AddedFromIP"];
                    dr["UpdatedBy"] = dataSet.Tables[0].Rows[i]["UpdatedBy"];
                    dr["UpdatedDate"] = dataSet.Tables[0].Rows[i]["UpdatedDate"];
                    dr["UpdatedFromIP"] = dataSet.Tables[0].Rows[i]["UpdatedFromIP"];

                    dsOperation.Tables[0].Rows.Add(dr);
                }
            }
        }

        #endregion
    }
}