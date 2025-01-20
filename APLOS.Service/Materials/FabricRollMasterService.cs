#region Using

//using IronBarCode;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Zen.Barcode;

#endregion Using

namespace Library.Service.Materials
{
    public class FabricRollMasterService : Service<FabricRollMaster>, IFabricRollMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<FabricRollMasterIncrementValue> _fabricRollMasterIncrementValue;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFabricRollMasterDefectService _fabricRollMasterDefectService;

        public FabricRollMasterService(
            IRepositoryAsync<FabricRollMasterIncrementValue> fabricRollMasterIncrementValue
            , IRepositoryAsync<FabricRollMaster> FabricRollMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IFabricRollMasterDefectService fabricRollMasterDefectService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(FabricRollMasterRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _fabricRollMasterDefectService = fabricRollMasterDefectService;
            _fabricRollMasterIncrementValue = fabricRollMasterIncrementValue;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(FabricRollMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(FabricRollMaster entity)
        {
            try
            {
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public void InsertOrUpdateGraph(IEnumerable<FabricRollMaster> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please select paid hours employee assign");
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(FabricRollMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        InsertGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var v = GetFabricIncrementValue(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, identity.PlantId).Max();
                v.IncrementValue = v.IncrementValue + entities.Count();
                _fabricRollMasterIncrementValue.Update(v);
                string inventoryReceiveDetailId = entities.First().InventoryReceiveDetailId;
                var dbList = base.Query(t => t.InventoryReceiveDetailId == inventoryReceiveDetailId).Select().ToList();
                if (dbList != null && dbList.Count() > 0)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
                //throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                //null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetFabricRollMasterPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FabricRollMaster", out sID);
            return sID;
        }
        public void UpdateFabricRoll(List<Dictionary<string, object>> FabricRollData,string PackingForm)
        {
            DataSet dsFabricRoll;
            string _Id = GetFabricRollMasterPK();

            ConnectionManager.DAL.ConManager conFabricRoll = new ConnectionManager.DAL.ConManager("1");
            conFabricRoll.OpenDataSetThroughAdapter("select * from TRN.FabricRollMaster where InventoryReceiveDetailId='" + FabricRollData[0]["InventoryReceiveDetailId"] + "'", out dsFabricRoll, false, "1");
            conFabricRoll.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where id='" + FabricRollData[0]["InventoryReceiveDetailId"] + @"'", out DataSet dsPackingForm, false, "1");
            int count = 0;
            if (FabricRollData != null)
            {
                foreach (var item in FabricRollData)
                {
                    count++;
                    DataView dv = new DataView(dsFabricRoll.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.BeginEdit();
                        drmo["RollNo"] = item["RollNo"];
                        drmo["VendorRollNo"] = item["VendorRollNo"];
                        drmo["VendorWidth"] = item["VendorWidth"];
                        drmo["VendorLotNo"] = item["VendorLotNo"];
                        drmo["VendorQty"] = item["VendorQty"];
                        drmo.EndEdit();
                    }
                }
            }
            if (dsPackingForm.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < dsPackingForm.Tables[0].Rows.Count; j++)
                {
                    dsPackingForm.Tables[0].DefaultView.RowFilter = "Id='" + dsPackingForm.Tables[0].Rows[j]["Id"].ToString() + "'";

                    if (dsPackingForm.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow drPf = dsPackingForm.Tables[0].DefaultView[0].Row;
                        drPf.BeginEdit();
                        drPf["PackingForm"] = PackingForm;
                        drPf.EndEdit();
                    }
                }
            }


            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsFabricRoll, dsPackingForm);

        }
        public void CreateRoll(int NoofRolls, Dictionary<string, object> SelectedRow, double Width, string PackingForm)
        {
            DataSet dsFabricRoll;
            string _Id = GetFabricRollMasterPK();
            string Plantid = "";
            string RollPrefix = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager conRoll = new ConnectionManager.DAL.ConManager("1");
            conRoll.OpenDataSetThroughAdapter("select * from TRN.FabricRollMaster where InventoryReceiveDetailId='" + SelectedRow["Id"] + "'", out dsFabricRoll, false, "1");
            conRoll.OpenDataSetThroughAdapter("select * from scs.PlantConfig where PlantId='" + SelectedRow["PlantId"] + @"'", out DataSet dsPlant, false, "1");
            conRoll.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where id='" + SelectedRow["Id"] + @"'", out DataSet dsPackingForm, false, "1");
            if (dsPlant.Tables[0].Rows.Count > 0)
                RollPrefix = dsPlant.Tables[0].Rows[0]["FabRollPrefix"].ToString();
            string sID = "";
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenIDDaily(DateTime.Now.ToShortDateString().ToString(), "FabricRollMaster" + Plantid, out sID);

            for (int i = 1; i <= NoofRolls; i++)
            {
                DataRow dr = dsFabricRoll.Tables[0].NewRow();               

                string RollNo = RollPrefix;
                RollNo += System.DateTime.Now.ToString("yyyy");
                RollNo += System.DateTime.Now.ToString("MM");
                RollNo += System.DateTime.Now.ToString("dd");
                RollNo += Convert.ToInt32(clsStaticInfo.dbl(sID)).ToString("D4");
                RollNo += Convert.ToInt32(OTSBD.clsStaticInfo.dbl(i.ToString())).ToString("D4");

                dr["Id"] = "R" + sID + "-" + i;
                dr["RollNo"] = RollNo;
                dr["VendorQty"] = Convert.ToInt32( SelectedRow["TransactionQty"].ToString()) / NoofRolls;
                dr["MaterialMasterId"] = SelectedRow["MaterialMasterId"]; 
                dr["ArticleId"] = SelectedRow["ArticleId"];
                dr["PlantId"] = SelectedRow["PlantId"];
                dr["InventoryReceiveDetailId"] = SelectedRow["Id"];
                dr["VendorWidth"] = Width;


                dsFabricRoll.Tables[0].Rows.Add(dr);

            }
            if (dsPackingForm.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < dsPackingForm.Tables[0].Rows.Count; j++)
                {
                    dsPackingForm.Tables[0].DefaultView.RowFilter = "Id='" + dsPackingForm.Tables[0].Rows[j]["Id"].ToString()+"'";

                    if (dsPackingForm.Tables[0].DefaultView.Count > 0)
                    {
                        //edit
                        DataRow drPf = dsPackingForm.Tables[0].DefaultView[0].Row;
                        drPf.BeginEdit();
                        drPf["PackingForm"] = PackingForm;
                        drPf.EndEdit();
                    }
                }
            }
            clsStaticInfo _info = new clsStaticInfo();
            _info.SaveDataSets(dsFabricRoll, dsPackingForm);
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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



        public int InsertOrUpdateGraphIncrement()
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var PlantId = identity.PlantId;
                var v = GetFabricIncrementValue(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, PlantId).Max();
                if (v == null)
                {
                    FabricRollMasterIncrementValue fabricRollMasterIncrementValue = new FabricRollMasterIncrementValue()
                    {
                        Year = DateTime.Now.Year,
                        Month = DateTime.Now.Month,
                        Day = DateTime.Now.Day,
                        IncrementValue = 1,
                        PlantId = PlantId
                    };
                    _fabricRollMasterIncrementValue.Insert(fabricRollMasterIncrementValue);
                }
                else
                {
                    v.IncrementValue = v.IncrementValue;
                    _fabricRollMasterIncrementValue.Update(v);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                if (v == null)
                {
                    return 1;
                }
                else
                {

                }
                return v == null ? 1 : v.IncrementValue;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public override void Update(FabricRollMaster entity)
        {
            try
            {
                var db = GetDBList(entity.Id).FirstOrDefault();
                db.Id = entity.Id;
                db.PlantId = entity.PlantId;
                db.RollNo = entity.RollNo;
                db.HasDefectShade = entity.HasDefectShade;
                db.ShrinkagePercentageWidth = entity.ShrinkagePercentageWidth;
                db.Shade = entity.Shade;
                db.IAddedDate = DateTime.Now;
                db.IAddedBy = entity.IAddedBy;
                db.IAddedFromIP = entity.IAddedFromIP;
                db.IUpdatedBy = entity.IUpdatedBy;
                db.IUpdatedDate = DateTime.Now;
                db.IUpdatedFromIP = entity.IUpdatedFromIP;
                db.SpecialShadeType = entity.SpecialShadeType;
                db.QUpdatedBy = entity.QAddedBy;
                db.QUpdatedFromIP = entity.QAddedFromIP;
                db.BlanketLengthAfterWash = entity.BlanketLengthAfterWash;
                db.BlanketWidthAfterWash = entity.BlanketWidthAfterWash;
                db.ShrinkagePercentageLength = entity.ShrinkagePercentageLength;
                db.ShrinkagePercentageWidth = entity.ShrinkagePercentageWidth;
                db.QualityPass = entity.QualityPass;
                base.Update(db);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateFabricInitial(FabricRollMaster entity)
        {
            try
            {
                var db = GetDBList(entity.Id).FirstOrDefault();
                db.Width = entity.Width;
                db.VendorWidth = entity.VendorWidth;
                db.VendorRollNo = entity.VendorRollNo;
                db.VendorQty = entity.VendorQty;
                db.VendorLotNo = entity.VendorLotNo;
                db.IAddedFromIP = entity.IAddedFromIP;
                db.RollNo = entity.RollNo;
                db.Id = entity.Id;
                db.BlanketLengthBeforeWash = entity.BlanketLengthBeforeWash;
                db.BlanketWidthBeforeWash = entity.BlanketWidthBeforeWash;
                db.IsBlanketCutApplicable = entity.IsBlanketCutApplicable;
                db.IUpdatedDate = DateTime.Now;
                base.Update(db);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        //public void UpdateFabricInsPection(FabricRollMaster entity,IEnumerable<FabricRollMasterDefect> fabricRollMasterDefect)
        //{
        //    try
        //    {
        //        FabricRollMaster db = GetDBList(entity.Id).FirstOrDefault();
        //        db.Id = entity.Id;
        //        db.PlantId = entity.PlantId;
        //        db.RollNo = entity.RollNo;
        //        db.VendorRollNo = entity.VendorRollNo;
        //        db.IsInspectionPassed = entity.IsInspectionPassed;
        //        db.QtyInspected = entity.QtyInspected;
        //        db.InsAddedDate = DateTime.Now;
        //        db.InsAddedBy = entity.IAddedBy;
        //        db.InsAddedFromIP = entity.IAddedFromIP;
        //        db.InsUpdatedBy = entity.IUpdatedBy;
        //        db.InsUpdatedDate = DateTime.Now;
        //        db.InsUpdatedFromIP = entity.IUpdatedFromIP;
        //        base.Update(db);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //        Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
        //        ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}
        public void UpdateFabricInsPectionWithDefect(FabricRollMaster entity, FabricRollMasterDefect fabricRollMasterDefect)
        {
            var flag = false;
            try
            {
                //CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var db = GetDBList(entity.Id).FirstOrDefault();
                db.Id = entity.Id;
                db.PlantId = entity.PlantId;
                db.RollNo = entity.RollNo;
                db.IsInspectionPassed = entity.IsInspectionPassed;
                db.QtyInspected = entity.QtyInspected;
                db.InsUpdatedBy = entity.InsUpdatedBy;
                db.InsUpdatedDate = DateTime.Now;
                db.InsUpdatedFromIP = entity.InsUpdatedFromIP;
                db.Remark = entity.Remark;
                UpdateGraph(db);
                _fabricRollMasterDefectService.Insert(fabricRollMasterDefect);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void UpdateFabricInsPection(FabricRollMaster entity)
        {
            var flag = false;
            try
            {
                //CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var db = GetDBList(entity.Id).FirstOrDefault();
                db.Id = entity.Id;
                db.PlantId = entity.PlantId;
                db.RollNo = entity.RollNo;
                db.IsInspectionPassed = entity.IsInspectionPassed;
                db.QtyInspected = entity.QtyInspected;
                db.InsUpdatedBy = entity.InsUpdatedBy;
                db.InsUpdatedDate = DateTime.Now;
                db.InsUpdatedFromIP = entity.InsUpdatedFromIP;
                db.Remark = entity.Remark;
                UpdateGraph(db);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> QueryList(string value)
        {
            try
            {
                var _sql = @"SELECT
                                    f.Id,
									f.[MaterialMasterId]
									,ISNULL(a.BlanketLengthBeforeWash,0) SettingBlanketLengthBeforeWash
									,ISNULL(a.BlanketWidthBeforeWash,0) SettingBlanketWidthBeforeWash
                                    ,f.RollNo
                                    ,f.HasDefectShade
                                    ,ISNULL(f.SpecialShadeType,'') SpecialShadeType
                                    ,f.ShrinkagePercentageWidth
                                    ,ISNULL(f.Shade,'')  Shade
                                    ,f.Width
                                    ,ISNULL(f.VendorRollNo,'') VendorRollNo
                                    ,ISNULL(f.VendorLotNo,'') VendorLotNo
                                    ,f.VendorQty
                                    ,f.VendorWidth
                                    ,f.PlantId
                                    ,f.QualityPass
                                    ,f.IsBlanketCutApplicable
                                    ,f.BlanketLengthBeforeWash
                                    ,f.BlanketWidthBeforeWash
                                    ,f.BlanketLengthAfterWash
                                    ,f.BlanketWidthAfterWash
                                    ,f.QtyInspected
                                    ,ISNULL(f.Remark,'') Remark
                                    ,f.IsInspectionPassed
									,CASE ISNULL(P.IsAfterWashShrinkageOnActual,'') when '' then CAST('False' as bit) else P.IsAfterWashShrinkageOnActual end IsAfterWashShrinkageOnActual
									,M.UserName MaterialMasterName
									,ISNULL(MA.StandardName,'') ArticleName
									,ISNULL(C.UserName,'') SkuName
									,ISNULL(CV.UserName,'') SkuValueName
									,f.ArticleId
									,f.SKUId
									,f.SKUValueId
                                    FROM
                                    TRN.FabricRollMaster f
									left outer join mst.FabricRollManagementSettings a on a.MaterialMasterId=f.MaterialMasterId
									LEFT OUTER JOIN SCS.PlantConfig P ON F.PlantId=P.PlantId
									LEFT OUTER JOIN MST.MaterialMaster M ON F.MaterialMasterId=M.Id
									LEFT JOIN MST.MaterialMasterArticle MA ON F.ArticleId=MA.Id
									LEFT JOIN HKP.Characteristics C ON F.SKUId=C.Id
									LEFT JOIN HKP.CharacteristicsValue CV ON F.SKUValueId=CV.Id
                                    WHERE RollNo='" + value + "'";
                var a = _sqlRepository.GetDataCollection(_sql, null);
                return a;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<FabricRollMasterDefectViewModel> QueryRollMasterDefectList(string value)
        {
            try
            {
                var _sql = @"SELECT MD.Id,MD.DefectCount,MD.FabricRollMasterId,D.Code,MD.DefectCodeId FROM [MST].[FabricRollMasterDefect] MD
                                LEFT OUTER JOIN [MST].[DefectCode] D ON MD.DefectCodeId=D.Id
                                WHERE MD.FabricRollMasterId='" + value + "'";
                var a = _sqlRepository.GetModelCollection<FabricRollMasterDefectViewModel>(_sql, null);
                return a;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public object QueryFriPlantConfigInfo()
        {
            try
            {
                var _sql = @"select BlanketDefaultLength,BlanketDefaultWidth,IsBlanketDefaultLengthValuesChangeable,IsBlanketDefaultWidthValuesChangeable,IsAfterWashShrinkageOnActual from SCS.PlantConfig";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<FabricRollMaster> GetDBList(string id)
        {
            try
            {
                var _sql = @" SELECT
                                    f.*
                                    FROM
                                    TRN.FabricRollMaster f
									where 	f.Id ='" + id + "'";
                return _sqlRepository.GetModelCollection<FabricRollMaster>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private IEnumerable<FabricRollMasterIncrementValue> GetFabricIncrementValue(int year, int month, int day, string PlantId)
        {
            try
            {
                var _sql = @"SELECT * FROM SCS.[FabricRollMasterIncrementValue] WHERE [YEAR]=" + year + " AND [MONTH]=" + month + " AND [DAY]=" + day + " AND PlantId=" + PlantId + "";
                return _sqlRepository.GetModelCollection<FabricRollMasterIncrementValue>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        //public IEnumerable<object> GetCbo()
        //{
        //    try
        //    {
        //        return from m in base.Query().Select().OrderBy(r => r.UserName)
        //               select new { Text = m.UserName, Value = m.Id };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //            ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
        //    }
        //}
        public IEnumerable<object> GetDefectCodeList()
        {
            try
            {
                var _sql = @"SELECT D.Id [Value], D.Code [Text], D.Description FROM [MST].[DefectCode] D where D.Archive=0 and D.Active=1";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public GridModel Query(GridParameter parameters, string companyGroupId, string paidHours, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT PG.*,E.EmployeeName,E.EmployeeCode,E.GivenDesignationId,PR.DepartmentId,PR.DivisionId,PR.SectionId,EC.Id EmployeeCategoryId,EC.UserName EmployeeCategory,GD.UserName GivenDesignation,D.UserName Department,DV.UserName Division,S.UserName Section FROM [MST].[PaidHoursEmployeeAssign] PG
                                        LEFT JOIN EmployeeInformation E ON PG.EmployeeId=E.SystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
										LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                        LEFT JOIN ORG.Department D ON PR.DepartmentId=D.Id
                                        LEFT JOIN ORG.Division DV ON PR.DivisionId=DV.Id
                                        LEFT JOIN ORG.Section S ON PR.SectionId= S.Id
										LEFT JOIN MST.DesignationMaster dmt ON dmt.DesignationId=E.GivenDesignationId
										LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=dmt.EmployeeCategoryId
                                        WHERE PG.PaidHours='" + paidHours + "' and PG.CompanyGroupId='" + companyGroupId + "' AND PG.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public GridModel GetGRNList(GridParameter parameters, string fabricRoll)



        {
            try
            {
                parameters.CmdText = @"SELECT IR.Id GRNNo,IR.Id,IR.CompanyGroupId,IR.CompanyId,IR.PlantId,P.UserName PartyName,SUM(IRD.TransactionQty) TotalDetailQty,SUM(IRD.MaterialTranAmount) TotalDetailAmount,C.Code Currency, REPLACE(Convert(VARCHAR(11), IR.GRNDate, 106), ' ', '-')  GRNDate,po.Id AS POID,po.PODate,C.Code FROM TRN.InventoryReceive IR
										LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
										LEFT JOIN TRN.InventoryReceiveDetail IRD ON IR.Id=IRD.InventoryReceiveId
                                        LEFT JOIN TRN.PurchaseOrder po on po.id=IRD.POId
										LEFT JOIN SCS.Currency C ON IR.CurrencyId=C.Id
									    LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
                                        WHERE BP.BusinessProcessName='FabricRollManagement'
										GROUP BY IR.Id,IR.CompanyGroupId,IR.CompanyId,IR.PlantId,P.UserName,IR.GRNDate,C.Code,po.Id,po.PODate";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetGRNDetailList(GridParameter parameters, string inventoryReceiveId, string fabricRoll)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,IR.Id GRNNo,IR.GRNDate,P.UserName PartyName,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName,C.UserName SKUName,CV.UserName SKUValue, C.UserName +':'+CV.UserName SKUInfo,CU.Code FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										--LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id
                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty,InventoryReceiveDetailId FROM TRN.FabricRollMaster GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
                                        WHERE BP.BusinessProcessName='FabricRollManagement' AND IRD.InventoryReceiveId='" + inventoryReceiveId + @"'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }
        public GridModel GetFABRollList(GridParameter parameters, string inventoryReceiveDetailId)
        {
            try
            {
                parameters.CmdText = @"SELECT FRM.*,IRD.TransactionQty,P.UserName PartyName,PL.FilePrefix,MM.UserName MaterialMasterName,MMA.StandardName ArticleName,C.UserName SKUName,CV.UserName SKUValue, C.UserName +':'+CV.UserName SKUInfo FROM TRN.FabricRollMaster FRM
                                        LEFT JOIN TRN.InventoryReceiveDetail IRD ON FRM.InventoryReceiveDetailId=IRD.Id
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
                                        LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN MST.MaterialMaster MM  ON IM.MaterialMasterId=MM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id
                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        WHERE FRM.InventoryReceiveDetailId='" + inventoryReceiveDetailId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }
        public IEnumerable<object> GetBarCideList(string inventoryReceiveDetailId)
        {
            try
            {
                var _sql = @"SELECT FRM.*,M.UserName MaterialMasterName,MMA.StandardName ArticleName,P.UserName PartyName,IR.Id GRNNo,CH.UserName SkuName,CV.UserName SkuValue,FRM.IsBlanketCutApplicable,FRM.ShrinkagePercentageWidth,FRM.Shade  from [TRN].[FabricRollMaster] FRM 
                            LEFT JOIN MST.MaterialMaster M on FRM.MaterialMasterId=M.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON FRM.ArticleId=MMA.Id
                            LEFT JOIN TRN.InventoryReceiveDetail IRD ON FRM.InventoryReceiveDetailId=IRD.Id
                            LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN HKP.Party P ON IR.PartyId= P.Id
                            LEFT JOIN HKP.Characteristics CH ON FRM.SKUId=CH.Id
                            LEFT JOIN HKP.CharacteristicsValue CV ON FRM.SKUValueId=CV.Id
                            where FRM.InventoryReceiveDetailId='" + inventoryReceiveDetailId + "'";
                List<object> lS = new List<object>();
                var s = _sqlRepository.GetDataCollection(_sql, null);
                foreach (var item in s)
                {
                    var dic = item;
                    string b = getBarCodePdf(dic["RollNo"].ToString());
                    var v = new
                    {
                        barCode = b,
                        RollNo = dic["RollNo"].ToString(),
                        Party = dic["PartyName"].ToString(),
                        GRNNo = dic["GRNNo"].ToString(),
                        MaterialName = dic["MaterialMasterName"].ToString(),
                        ArticleName = dic["ArticleName"].ToString(),
                        SkuName = dic["SkuName"].ToString(),
                        SkuValue = dic["SkuValue"].ToString(),
                        QualityPass = Convert.ToBoolean(dic["QualityPass"].ToString()),
                        VendorLotNo = dic["VendorLotNo"].ToString(),
                        VendorQty = Convert.ToDecimal(dic["VendorQty"].ToString()),
                        ShrinkagePercentageWidth = Convert.ToDecimal(dic["ShrinkagePercentageWidth"].ToString()),
                        Shade = dic["Shade"].ToString(),
                    };
                    lS.Add(v);
                }
                return lS;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string getBarCodePdf(string barcode)
        {
            try
            {
                // barcode = "19628000001";
                //GeneratedBarcode MyBarCode = IronBarCode.BarcodeWriter.CreateBarcode(barcode, BarcodeWriterEncoding.Code128);
                //MyBarCode.SaveAsJpeg("MyBarCode.jpg");
                //Image MyBarCodeImage = MyBarCode.Image;
                //using (Image image = MyBarCodeImage)
                //{
                //    using (MemoryStream m = new MemoryStream())
                //    {
                //        image.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
                //        byte[] imageBytes = m.ToArray();

                //        // Convert byte[] to Base64 String
                //        string base64String = Convert.ToBase64String(imageBytes);
                //        return "data:image/png;base64," + base64String;
                //    }
                //}

                Code128BarcodeDraw barCode128 = BarcodeDrawFactory.Code128WithChecksum;
                Image MyBarCodeImage = barCode128.Draw(barcode, 52, 1);
                using (Image image = MyBarCodeImage)
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        return "data:image/png;base64," + base64String;
                    }
                }

                //using (MemoryStream memoryStream = new MemoryStream())
                //{
                //    using (Bitmap bitMap = new Bitmap(barcode.Length * 40, 80))
                //    {
                //        using (Graphics graphics = Graphics.FromImage(bitMap))
                //        {
                //            Font oFont = new Font("IDAutomationHC39M", 16);
                //            PointF point = new PointF(2f, 2f);
                //            SolidBrush whiteBrush = new SolidBrush(Color.White);
                //            graphics.FillRectangle(whiteBrush, 0, 0, bitMap.Width, bitMap.Height);
                //            SolidBrush blackBrush = new SolidBrush(Color.DarkBlue);
                //            graphics.DrawString(barcode , oFont, blackBrush, point);
                //        }
                //        bitMap.Save(memoryStream, ImageFormat.Jpeg);
                //        return  "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        //public void DownloadBarcode_Excel(int LOW, int HIGH, HttpResponse response, string GRNSKUSystemID)
        //{

        //    // initialize the Class of Query 
        //    System.Data.DataSet dsCompany = null;

        //    clsPurchaseOrder objPur = null;
        //    clsStaticInfo objStatic = null;


        //    ExcelEngine excelEngine = null;
        //    IApplication application = null;
        //    IWorkbook workbook = null;
        //    IWorksheet sheet = null;
        //    string address = "";
        //    try
        //    {
        //        objStatic = new clsStaticInfo();
        //        objPur = new clsPurchaseOrder();

        //        DataSet dsItems = null;
        //        objPur.GetGRNBarCode(LOW, HIGH, GRNSKUSystemID, out dsItems);
        //        if (dsItems.Tables[0].Select("isnull(PackingForm,'')='ROLL' OR isnull(PackingForm,'')='BALE' ").Length == 0)
        //        {
        //            Exception ex = new Exception("No Data Found!!!");
        //            throw (ex);
        //        }

        //        string companyID = dsItems.Tables[0].Rows[0]["CompanyID"].ToString();

        //        objPur.getCompanyBySystemID(companyID, out dsCompany);



        //        excelEngine = new ExcelEngine();
        //        application = excelEngine.Excel;
        //        workbook = application.Workbooks.Create(4);
        //        workbook.Worksheets[0].Name = "Barcode-(" + dsItems.Tables[0].Rows[0]["GRNMasterSystemID"].ToString() + ")Color(" + dsItems.Tables[0].Rows[0]["SKU"].ToString() + ")";
        //        sheet = workbook.Worksheets[0];

        //        int ROW = 1;
        //        int COLLeft = 1;
        //        int COLRight = 4;
        //        int endCol = 6;
        //        int startRow = ROW;
        //        int BarcodeCount = Convert.ToInt32(Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[0]["numberOfPackages"].ToString())).ToString("F0"));
        //        double seq = 0;
        //        string _gateEntryDate = "";
        //        IPictureShape pic = null;
        //        for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
        //        {
        //            seq = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["SEQ"].ToString()));
        //            if (HIGH > 0)
        //            {
        //                if (seq < LOW || seq > HIGH)
        //                    continue;
        //            }

        //            if (i > 0)
        //                sheet.HPageBreaks.Add(sheet.Range[ROW, 1]);

        //            Code128BarcodeDraw barCode128 = BarcodeDrawFactory.Code128WithChecksum;
        //            System.Drawing.Image barcodeImg = barCode128.Draw(dsItems.Tables[0].Rows[i]["PackingFormNo"].ToString(), 52, 2);


        //            ROW++;

        //            sheet.Range[ROW - 1, COLLeft, ROW, endCol - 2].Merge();
        //            sheet.Range[ROW - 1, endCol - 1, ROW, endCol].Merge();

        //            sheet[ROW - 1, endCol - 1].Text = seq.ToString("F0") + "/" + BarcodeCount.ToString();
        //            sheet[ROW - 1, COLLeft].Text = dsItems.Tables[0].Rows[i]["FileNos"].ToString();
        //            sheet.Range[ROW - 1, COLLeft, ROW, endCol].CellStyle.Font.Size = 12f;
        //            sheet.Range[ROW - 1, COLLeft, ROW, endCol].CellStyle.Font.Bold = true;
        //            sheet.Range[ROW - 1, COLLeft, ROW, endCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        //            sheet.Range[ROW - 1, COLLeft, ROW, endCol - 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //            sheet.Range[ROW - 1, endCol - 1, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignRight;

        //            ROW++;
        //            //if (barcodeImg.Width > 300)
        //            //{
        //            //    sheet.Range[ROW, COLLeft, ROW, COLRight - 1].Merge();
        //            //    sheet[ROW, COLLeft].Text = dsItems.Tables[0].Rows[i]["FileNos"].ToString();
        //            //}
        //            //else
        //            //{
        //            //    sheet.Range[ROW, COLLeft + 1, ROW, COLRight - 1].Merge();
        //            //    sheet[ROW, COLLeft + 1].Text = dsItems.Tables[0].Rows[i]["FileNos"].ToString();
        //            //}




        //            sheet.Range[ROW, COLLeft, ROW, endCol].Merge();
        //            sheet.Range[ROW, COLLeft, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //            sheet.Range[ROW, COLLeft, ROW, endCol].Text = dsItems.Tables[0].Rows[i]["PackingFormNo"].ToString();
        //            sheet.Range[ROW, COLLeft, ROW, endCol].CellStyle.Font.Size = 28f;
        //            sheet.Range[ROW, COLLeft, ROW, endCol].RowHeight = 34.5f;
        //            sheet.Range[ROW, COLLeft, ROW, endCol].CellStyle.Font.Bold = true;

        //            ROW++;
        //            startRow = ROW;

        //            int QCPlacementCol = 1;
        //            pic = sheet.Pictures.AddPicture(ROW, 2, barcodeImg);
        //            pic.Width = 300;// (int)(2 * 96);//2 inch 96dpi
        //            QCPlacementCol = endCol;
        //            //if (barcodeImg.Width > 300)
        //            //{
        //            //    //pic = sheet.Pictures.AddPicture(ROW, 1, barcodeImg);
        //            //    pic = sheet.Pictures.AddPicture(ROW, 2, barcodeImg);
        //            //    pic.Width = (int)(2 * 96);//2 inch 96dpi
        //            //    QCPlacementCol = endCol;
        //            //}
        //            //else
        //            //{
        //            //    pic = sheet.Pictures.AddPicture(ROW, 2, barcodeImg);
        //            //    pic.Width = (int)(2 * 96);//2 inch 96dpi
        //            //}


        //            #region QC

        //            if (dsItems.Tables[0].Rows[i]["isQualityDone"].ToString() == "YES")
        //            {
        //                sheet[ROW, QCPlacementCol].Text = "QC";
        //                if (dsItems.Tables[0].Rows[i]["QCStatus"].ToString() == "")
        //                    sheet[ROW + 1, QCPlacementCol].Text = "N/A";
        //                else
        //                    sheet[ROW + 1, QCPlacementCol].Text = dsItems.Tables[0].Rows[i]["QCStatus"].ToString().ToUpper();
        //            }
        //            else
        //            {
        //                sheet[ROW, QCPlacementCol].Text = "PRE";
        //                sheet[ROW + 1, QCPlacementCol].Text = "QC";
        //            }

        //            sheet.Range[ROW, QCPlacementCol, ROW + 1, QCPlacementCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        //            sheet.Range[ROW, QCPlacementCol, ROW + 1, QCPlacementCol].CellStyle.Font.Bold = true;
        //            #endregion QC

        //            ROW += 3;
        //            _gateEntryDate = "";
        //            if (bplib.clsWebLib.makeBaseBlank(Convert.ToDateTime(dsItems.Tables[0].Rows[0]["GateEntryDate"].ToString()).ToString("dd-MMM-yyyy")) != "")
        //                _gateEntryDate = " (Gt.Ent.Dt: " + dsItems.Tables[0].Rows[i]["GateEntryDate"].ToString() + ")";

        //            sheet.Range[ROW, COLLeft, ROW, endCol].Merge();
        //            sheet[ROW, COLLeft].RowHeight = sheet[ROW, COLLeft].RowHeight * 3;
        //            sheet[ROW, COLLeft].Text = dsItems.Tables[0].Rows[i]["MaterialDesc"].ToString() + _gateEntryDate;
        //            sheet.Range[ROW, COLLeft, ROW, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //            sheet.Range[ROW, COLLeft, ROW, endCol].WrapText = true;

        //            ROW++;
        //            int ResetROW = ROW;
        //            sheet[ROW, COLLeft].Text = dsItems.Tables[0].Rows[i]["SKU"].ToString();
        //            sheet.Range[ROW, COLLeft, ROW, COLRight - 1].Merge();
        //            sheet.Range[ROW, COLLeft, ROW, COLRight - 1].CellStyle.Font.Bold = true;

        //            ROW = ResetROW;
        //            //right
        //            sheet[ROW, COLRight].Text = "CI#";
        //            sheet[ROW, COLRight].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[ROW, COLRight + 1, ROW, endCol].Merge();
        //            sheet[ROW, COLRight + 1].Text = dsItems.Tables[0].Rows[i]["InvoiceNumber"].ToString();


        //            ROW++;
        //            sheet.Range[ROW, COLLeft, ROW, endCol].Merge();
        //            sheet[ROW, COLLeft].Text = "Vendor: " + dsItems.Tables[0].Rows[i]["Vendor"].ToString();
        //            sheet.Range[ROW, COLLeft, ROW, endCol].WrapText = true;


        //            ResetROW = ROW;
        //            ROW++;

        //            sheet[ROW, COLLeft].Text = "S.R#" + dsItems.Tables[0].Rows[i]["VendorPackingFormNo"].ToString() + " Lt#" + dsItems.Tables[0].Rows[i]["VendorLotNo"].ToString();
        //            sheet.Range[ROW, COLLeft, ROW, COLRight - 1].Merge();
        //            sheet.Range[ROW, COLLeft, ROW, COLRight - 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //            sheet.Range[ROW, COLLeft, ROW, COLRight - 1].CellStyle.Font.Bold = true;

        //            ROW = ResetROW;
        //            ROW++;
        //            sheet[ROW, COLRight].Text = "PL Qty";
        //            sheet.Range[ROW, COLRight + 1, ROW, endCol].Merge();
        //            sheet[ROW, COLRight + 1].Text = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["PackingListQuantity"].ToString())).ToString("N2") + " " + dsItems.Tables[0].Rows[i]["UOM"].ToString();
        //            sheet[ROW, COLRight + 1].CellStyle.Font.Bold = true;
        //            if (dsItems.Tables[0].Rows[i]["FLAGReceivedQty"].ToString().ToUpper() == "YES")
        //            {
        //                sheet[ROW, COLRight + 1].Text = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["PackingListQuantity"].ToString())).ToString("N")
        //                    + "(" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["ReceivedQuantity"].ToString())).ToString("N") + ") " + dsItems.Tables[0].Rows[i]["UOM"].ToString();
        //                sheet.Range[ROW, COLRight + 1, ROW, endCol].CellStyle.Font.Size = 8f;
        //            }
        //            //if (FLAG.ToUpper() == "QC")
        //            //{
        //            if (dsItems.Tables[0].Rows[i]["isQualityDone"].ToString() == "YES")
        //            {
        //                ROW++;
        //                ResetROW = ROW;


        //                //sheet[ROW, COLLeft].Text = "QC.Shd-" + dsItems.Tables[0].Rows[i]["QCShade"].ToString() + dsItems.Tables[0].Rows[i]["QCSubShade"].ToString() + " M.Shd-" + dsItems.Tables[0].Rows[i]["MerchShade"].ToString() + dsItems.Tables[0].Rows[i]["MerchSubShade"].ToString();
        //                //sheet.Range[ROW, COLLeft, ROW, COLRight - 1].Merge();
        //                //sheet.Range[ROW, COLLeft, ROW, COLRight - 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //                //ROW++;
        //                sheet[ROW, COLLeft].Text = "Shade(M)";
        //                sheet.Range[ROW, COLLeft + 1, ROW, COLRight - 1].Merge();
        //                sheet[ROW, COLLeft + 1].Text = dsItems.Tables[0].Rows[i]["MerchShade"].ToString() + dsItems.Tables[0].Rows[i]["MerchSubShade"].ToString();
        //                sheet.Range[ROW, COLLeft + 1, ROW, COLRight - 1].CellStyle.Font.Bold = true;
        //                ROW++;
        //                sheet[ROW, COLLeft].Text = "Shrinkage";
        //                sheet.Range[ROW, COLLeft + 1, ROW, COLRight - 1].Merge();
        //                sheet[ROW, COLLeft + 1].Text = dsItems.Tables[0].Rows[i]["QCShrinkageGroup"].ToString();
        //                sheet.Range[ROW, COLLeft + 1, ROW, COLRight - 1].CellStyle.Font.Bold = true;
        //                ROW++;

        //                ROW = ResetROW;
        //                sheet[ROW, COLRight].Text = "QC Qty";
        //                sheet.Range[ROW, COLRight + 1, ROW, endCol].Merge();
        //                sheet[ROW, COLRight + 1].Text = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["ActualQuantity"].ToString())).ToString("N2") + " " + dsItems.Tables[0].Rows[i]["UOM"].ToString();
        //                sheet[ROW, COLRight + 1].CellStyle.Font.Bold = true;
        //                ROW++;
        //                sheet[ROW, COLRight].Text = "Cut. W.";
        //                sheet.Range[ROW, COLRight + 1, ROW, endCol].Merge();
        //                sheet[ROW, COLRight + 1].Text = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["CW"].ToString())).ToString("N2");
        //                sheet[ROW, COLRight + 1].CellStyle.Font.Bold = true;

        //            }
        //            //}

        //            ROW++;
        //            sheet[ROW, COLLeft].Text = "Vn.Spec";
        //            sheet.Range[ROW, COLLeft + 1, ROW, COLRight - 1].Merge();
        //            sheet[ROW, COLLeft + 1].Text = dsItems.Tables[0].Rows[i]["POVendorSpec"].ToString();


        //            sheet[ROW, COLRight].Text = "F.FType";
        //            sheet[ROW, COLRight].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[ROW, COLRight + 1, ROW, endCol].Merge();
        //            sheet[ROW, COLRight + 1].Text = dsItems.Tables[0].Rows[i]["FabricFinishType"].ToString();
        //            sheet[ROW, COLRight + 1].CellStyle.Font.Bold = true;
        //            ROW++;

        //            sheet.Range[startRow, COLLeft, ROW, endCol].VerticalAlignment = ExcelVAlign.VAlignTop;
        //        }


        //        sheet[ROW, 4].ColumnWidth = 6.25;
        //        sheet[ROW, 5].ColumnWidth = 9.50;
        //        sheet[ROW, endCol].ColumnWidth = 7;

        //        sheet.Protect(bplib.clsWebLib.REPORT_LOCK_PASSWORD);


        //        sheet.PageSetup.TopMargin = 0;
        //        sheet.PageSetup.BottomMargin = 0;
        //        //sheet.PageSetup.PrintTitleRows = "$1:$6";
        //        sheet.PageSetup.LeftMargin = .08;
        //        sheet.PageSetup.RightMargin = 0;
        //        //sheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
        //        //sheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Session["USER"].ToString() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
        //        sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
        //        sheet.PageSetup.FitToPagesTall = 0;
        //        sheet.PageSetup.FitToPagesWide = 1;
        //        sheet.PageSetup.PaperSize = ExcelPaperSize.PaperUser;
        //        //sheet.PageSetup.CenterHorizontally = true;


        //        workbook.Version = ExcelVersion.Excel97to2003;

        //        string range = "";
        //        if (HIGH > 0)
        //            range = "(" + LOW.ToString() + "-" + HIGH.ToString() + ")";
        //        string strFileName = "Barcode (" + (dsItems.Tables[0].Rows[0]["GRNMasterSystemID"].ToString()) + ") Color(" + dsItems.Tables[0].Rows[0]["SKU"].ToString() + ")" + System.DateTime.Today.ToString("dd-MMM-yyyy") + range + ".xls";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //        excelEngine.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {

        //    }

        //}

        //public void DownloadBarcode_PDF(int LOW, int HIGH, HttpResponse response, string GRNSKUSystemID, string user)
        //{


        //    Syncfusion.Pdf.PdfPage page = null;
        //    Syncfusion.Pdf.Graphics.PdfGraphics graphics = null;
        //    Syncfusion.Pdf.Graphics.PdfPen pen = null;
        //    Syncfusion.Pdf.Graphics.PdfSolidBrush brush = null;
        //    Syncfusion.Pdf.Graphics.PdfFont fontHead = null;
        //    Syncfusion.Pdf.Graphics.PdfFont fontBody = null;
        //    Syncfusion.Pdf.Graphics.PdfFont fontBody_Big = null;
        //    Syncfusion.Pdf.Graphics.PdfFont fontItalic = null;
        //    Syncfusion.Pdf.Graphics.PdfFont fontDesc = null;
        //    Syncfusion.Pdf.Graphics.PdfStringFormat formatCenter = null;
        //    Syncfusion.Pdf.Graphics.PdfStringFormat formatLeft = null;
        //    Syncfusion.Pdf.Graphics.PdfStringFormat formatRight = null;
        //    Syncfusion.Pdf.Graphics.PdfStringFormat formatLeft_Big = null;
        //    Syncfusion.Pdf.Barcode.PdfCode39Barcode barCode = null;

        //    PdfDocument doc = new Syncfusion.Pdf.PdfDocument();
        //    doc.Compression = PdfCompressionLevel.Normal;
        //    ///Document Settings----------------
        //    //Set page size
        //    doc.PageSettings.Size = PdfPageSize.A4;

        //    //doc.PageSettings.Size = new System.Drawing.SizeF(101, 50);

        //    //Set page orientation
        //    doc.PageSettings.Orientation = PdfPageOrientation.Portrait;
        //    // Add margins.
        //    doc.PageSettings.SetMargins(0f);
        //    //Info
        //    doc.DocumentInformation.CreationDate = System.DateTime.Now;
        //    doc.DocumentInformation.Creator = user;
        //    doc.DocumentInformation.Subject = "Barcode";
        //    doc.DocumentInformation.Title = "Barcode";
        //    ///----------------------------------------
        //    // initialize the Class of Query 
        //    System.Data.DataSet dsCompany = null;

        //    clsPurchaseOrder objPur = null;
        //    clsStaticInfo objStatic = null;


        //    string address = "";
        //    try
        //    {

        //        RectangleF Cell;
        //        string strText = "";
        //        fontHead = new PdfStandardFont(PdfFontFamily.TimesRoman, 12f, PdfFontStyle.Bold);
        //        PdfStandardFont fontPackingFormNo = new PdfStandardFont(PdfFontFamily.TimesRoman, 20f, PdfFontStyle.Bold);
        //        fontBody = new PdfStandardFont(PdfFontFamily.TimesRoman, 10f, PdfFontStyle.Bold);
        //        fontBody_Big = new PdfStandardFont(PdfFontFamily.TimesRoman, 22f, PdfFontStyle.Bold);
        //        fontItalic = new PdfStandardFont(PdfFontFamily.TimesRoman, 8f, PdfFontStyle.Italic);
        //        fontDesc = new PdfStandardFont(PdfFontFamily.TimesRoman, 10f, PdfFontStyle.Regular);
        //        PdfStandardFont fontDescBold = new PdfStandardFont(PdfFontFamily.TimesRoman, 10f, PdfFontStyle.Bold);
        //        //setting the string formation
        //        formatCenter = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Top);
        //        formatCenter.WordWrap = PdfWordWrapType.None;

        //        formatLeft = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
        //        formatLeft.WordWrap = PdfWordWrapType.None;

        //        PdfStringFormat formatLeftWrapped = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Top);
        //        formatLeftWrapped.WordWrap = PdfWordWrapType.Word;

        //        formatRight = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Top);
        //        formatRight.WordWrap = PdfWordWrapType.None;
        //        formatLeft_Big = new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle);
        //        //set the pen
        //        pen = new Syncfusion.Pdf.Graphics.PdfPen(PdfBrushes.Black, 0.5f);
        //        //set the brush
        //        brush = new Syncfusion.Pdf.Graphics.PdfSolidBrush(System.Drawing.Color.Black);

        //        objStatic = new clsStaticInfo();
        //        objPur = new clsPurchaseOrder();

        //        DataSet dsItems = null;
        //        objPur.GetGRNBarCode(LOW, HIGH, GRNSKUSystemID, out dsItems);
        //        if (dsItems.Tables[0].Select("isnull(PackingForm,'')='ROLL' OR isnull(PackingForm,'')='BALE' ").Length == 0)
        //        {
        //            Exception ex = new Exception("No Data Found!!!");
        //            throw (ex);
        //        }

        //        string companyID = dsItems.Tables[0].Rows[0]["CompanyID"].ToString();

        //        objPur.getCompanyBySystemID(companyID, out dsCompany);



        //        float ROW = 1;


        //        float startRow = ROW;
        //        float startingROW = ROW;
        //        int BarcodeCount = Convert.ToInt32(Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[0]["numberOfPackages"].ToString())).ToString("F0"));
        //        double seq = 0;


        //        //for PDF

        //        float pageStart = ROW;
        //        float blockWidth = 0;
        //        float blockHeight = 0;
        //        float rowHeight = 0;

        //        ROW = 0;
        //        page = doc.Pages.Add();

        //        graphics = page.Graphics;
        //        blockWidth = graphics.Size.Width / 2;
        //        blockHeight = graphics.Size.Height / 4;
        //        rowHeight = blockHeight / 16;


        //        pen.Width = 1f;
        //        pen.DashStyle = PdfDashStyle.Solid;
        //        graphics.DrawLine(pen, blockWidth, 0, blockWidth, graphics.Size.Height);



        //        int blockCount = 0;
        //        bool firstItemOfNewPage = true;

        //        float leftCol = 0;
        //        float rightCol = 0;
        //        float centerCol = 0;
        //        float endCol = 0;

        //        float leftMargin = 10;
        //        float rightColPadding = 10;

        //        float leftCellWidth = 0;
        //        float rightCellWidth = 0;
        //        for (int i = 0; i < dsItems.Tables[0].Rows.Count; i++)
        //        {
        //            seq = Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["SEQ"].ToString()));
        //            if (HIGH > 0)
        //            {
        //                if (seq < LOW || seq > HIGH)
        //                    continue;
        //            }
        //            #region locating page attributes
        //            if ((i + 1) > 1)
        //            {
        //                if ((i + 1) % 8 == 1)
        //                {

        //                    page = doc.Pages.Add();
        //                    graphics = page.Graphics;
        //                    blockWidth = (int)(graphics.Size.Width / 2);
        //                    blockHeight = (int)graphics.Size.Height / 4;
        //                    rowHeight = blockHeight / 16;
        //                    ROW = 0;

        //                    blockCount = 0;
        //                    firstItemOfNewPage = true;


        //                    pen.Width = 1f;
        //                    pen.DashStyle = PdfDashStyle.Solid;
        //                    graphics.DrawLine(pen, blockWidth, 0, blockWidth, graphics.Size.Height);
        //                }
        //            }

        //            if ((i + 1) % 2 == 0)
        //            {
        //                //RIGHT BLOCK
        //                leftCol = blockWidth + leftMargin;
        //                centerCol = (((float)graphics.Size.Width - leftMargin) * .75f);
        //                rightCol = centerCol + rightColPadding;
        //                endCol = graphics.Size.Width - leftMargin;
        //                ROW = startingROW;

        //            }
        //            else
        //            {
        //                //LEFT BLOCK
        //                leftCol = 0 + leftMargin;
        //                centerCol = (blockWidth - leftMargin) / 2;
        //                rightCol = centerCol + rightColPadding;
        //                endCol = blockWidth - leftMargin;



        //                if ((i + 1) > 1)
        //                {
        //                    if ((i + 1) % 2 != 0)
        //                    {
        //                        if (firstItemOfNewPage == false)
        //                        {
        //                            //FROM SECOND BLOCK, RESET EACH BLOCK TOP POSITION
        //                            blockCount++;
        //                            ROW = ((graphics.Size.Height / 4) * blockCount) + 1;
        //                        }

        //                    }
        //                }
        //                startingROW = ROW;

        //                firstItemOfNewPage = false;
        //            }


        //            leftCellWidth = rightCol - leftCol;
        //            rightCellWidth = (endCol - leftCol) - leftCellWidth;
        //            #endregion locating page attributes


        //            Cell = new RectangleF(leftCol, ROW, centerCol, rowHeight * 2);
        //            strText = dsItems.Tables[0].Rows[i]["FileNos"].ToString();
        //            graphics.DrawString(strText, fontHead, brush, Cell, formatLeft);


        //            strText = seq.ToString("F0") + "/" + BarcodeCount.ToString();
        //            graphics.DrawString(strText, fontHead, brush, endCol, ROW, formatRight);

        //            ROW += rowHeight;
        //            ROW += rowHeight;



        //            Cell = new RectangleF(leftCol, ROW, endCol, rowHeight * 2);
        //            strText = dsItems.Tables[0].Rows[i]["PackingFormNo"].ToString();
        //            graphics.DrawString(strText, fontPackingFormNo, brush, Cell, formatLeft);
        //            ROW += rowHeight;
        //            ROW += rowHeight;

        //            Code128BarcodeDraw barCode128 = BarcodeDrawFactory.Code128WithChecksum;
        //            System.Drawing.Image barcodeImg = barCode128.Draw(dsItems.Tables[0].Rows[i]["PackingFormNo"].ToString(), (int)rowHeight * 3, 2);
        //            PdfImage image = new PdfBitmap(barcodeImg);
        //            graphics.DrawImage(image, new PointF(leftCol, ROW));

        //            #region QC

        //            if (dsItems.Tables[0].Rows[i]["isQualityDone"].ToString() == "YES")
        //            {
        //                graphics.DrawString("QC", fontDescBold, brush, endCol, ROW, formatRight);

        //                if (dsItems.Tables[0].Rows[i]["QCStatus"].ToString() == "")
        //                    graphics.DrawString("N/A", fontDescBold, brush, endCol, ROW + rowHeight, formatRight);
        //                else
        //                    graphics.DrawString(dsItems.Tables[0].Rows[i]["QCStatus"].ToString().ToUpper(), fontDescBold, brush, endCol, ROW + rowHeight, formatRight);
        //            }
        //            else
        //            {
        //                graphics.DrawString("PRE", fontDescBold, brush, endCol, ROW, formatRight);
        //                graphics.DrawString("QC", fontDescBold, brush, endCol, ROW + rowHeight, formatRight);
        //            }

        //            #endregion QC

        //            ROW += rowHeight;
        //            ROW += rowHeight;
        //            ROW += rowHeight;

        //            Cell = new RectangleF(leftCol, ROW - rowHeight + 3, leftCellWidth + rightCellWidth, rowHeight * 3);
        //            strText = dsItems.Tables[0].Rows[i]["MaterialDesc"].ToString();
        //            graphics.DrawString(strText, fontDesc, brush, Cell, formatLeftWrapped);
        //            ROW += rowHeight;
        //            ROW += rowHeight;
        //            //ROW += rowHeight;

        //            float ResetROW = ROW;
        //            Cell = new RectangleF(leftCol, ROW, leftCellWidth, rowHeight);
        //            strText = dsItems.Tables[0].Rows[i]["SKU"].ToString();
        //            graphics.DrawString(strText, fontDescBold, brush, Cell, formatLeft);


        //            ROW = ResetROW;
        //            Cell = new RectangleF(rightCol, ROW, rightCellWidth, rowHeight);//centercol
        //            strText = "CI#" + dsItems.Tables[0].Rows[i]["InvoiceNumber"].ToString();
        //            graphics.DrawString(strText, fontDesc, brush, Cell, formatLeft);



        //            ROW += rowHeight;
        //            Cell = new RectangleF(leftCol, ROW, leftCellWidth + rightCellWidth, rowHeight);
        //            strText = "Vendor: " + dsItems.Tables[0].Rows[i]["Vendor"].ToString();
        //            graphics.DrawString(strText, fontDesc, brush, Cell, formatLeft);

        //            ResetROW = ROW;
        //            ROW += rowHeight;

        //            Cell = new RectangleF(leftCol, ROW, leftCellWidth, rowHeight);
        //            strText = "S.R#" + dsItems.Tables[0].Rows[i]["VendorPackingFormNo"].ToString() + " Lt#" + dsItems.Tables[0].Rows[i]["VendorLotNo"].ToString();
        //            graphics.DrawString(strText, fontDescBold, brush, Cell, formatLeft);

        //            ROW = ResetROW;
        //            ROW += rowHeight;
        //            strText = "PL Qty:" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["PackingListQuantity"].ToString())).ToString("N2") + " " + dsItems.Tables[0].Rows[i]["UOM"].ToString();

        //            if (dsItems.Tables[0].Rows[i]["FLAGReceivedQty"].ToString().ToUpper() == "YES")
        //            {
        //                strText = "PL Qty:" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["PackingListQuantity"].ToString())).ToString("N")
        //                    + "(" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["ReceivedQuantity"].ToString())).ToString("N") + ") " + dsItems.Tables[0].Rows[i]["UOM"].ToString();

        //            }
        //            Cell = new RectangleF(rightCol, ROW, rightCellWidth, rowHeight);
        //            graphics.DrawString(strText, fontDescBold, brush, Cell, formatLeft);



        //            if (dsItems.Tables[0].Rows[i]["isQualityDone"].ToString() == "YES")
        //            {
        //                ROW += rowHeight;
        //                ResetROW = ROW;
        //                Cell = new RectangleF(leftCol, ROW, leftCellWidth, rowHeight);
        //                strText = "Shade(M):" + dsItems.Tables[0].Rows[i]["MerchShade"].ToString() + dsItems.Tables[0].Rows[i]["MerchSubShade"].ToString();
        //                graphics.DrawString(strText, fontDesc, brush, Cell, formatLeft);
        //                ROW += rowHeight;
        //                Cell = new RectangleF(leftCol, ROW, leftCellWidth, rowHeight);
        //                strText = "Shrinkage:" + dsItems.Tables[0].Rows[i]["QCShrinkageGroup"].ToString();
        //                graphics.DrawString(strText, fontDesc, brush, Cell, formatLeft);
        //                ROW += rowHeight;


        //                ROW = ResetROW;
        //                strText = "QC Qty:" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["ActualQuantity"].ToString())).ToString("N2") + " " + dsItems.Tables[0].Rows[i]["UOM"].ToString();
        //                Cell = new RectangleF(rightCol, ROW, rightCellWidth, rowHeight);
        //                graphics.DrawString(strText, fontDescBold, brush, Cell, formatLeft);
        //                ROW += rowHeight;
        //                strText = "Cut. W." + Convert.ToDouble(bplib.clsWebLib.GetNumData(dsItems.Tables[0].Rows[i]["CW"].ToString())).ToString("N2");
        //                Cell = new RectangleF(rightCol, ROW, rightCellWidth, rowHeight);
        //                graphics.DrawString(strText, fontDescBold, brush, Cell, formatLeft);



        //            }
        //            //else
        //            //{
        //            //    ////pre QC, add two additional rows
        //            //    //ROW += rowHeight;
        //            //    //ROW += rowHeight;
        //            //}


        //            ROW += rowHeight;
        //            Cell = new RectangleF(leftCol, ROW, leftCellWidth, rowHeight);
        //            strText = "Vn.Spec:" + dsItems.Tables[0].Rows[i]["POVendorSpec"].ToString();
        //            graphics.DrawString(strText, fontDesc, brush, Cell, formatLeft);


        //            Cell = new RectangleF(rightCol, ROW, rightCellWidth, rowHeight);
        //            strText = "Rcv.Date:" + dsItems.Tables[0].Rows[i]["GRNDate"].ToString();
        //            graphics.DrawString(strText, fontDesc, brush, Cell, formatLeft);



        //            ROW += rowHeight;
        //            ROW += rowHeight;


        //            if ((i + 1) % 2 == 1)
        //            {
        //                if (dsItems.Tables[0].Rows[i]["isQualityDone"].ToString() != "YES")
        //                {
        //                    //pre QC, add two additional rows
        //                    ROW += rowHeight;
        //                    ROW += rowHeight;
        //                }

        //                pen.Width = 2.5f;
        //                pen.DashStyle = PdfDashStyle.DashDotDot;
        //                graphics.DrawLine(pen, 0, ROW - (rowHeight / 2), graphics.Size.Width, ROW - (rowHeight / 2));
        //            }
        //        }




        //        string range = "";
        //        if (HIGH > 0)
        //            range = "(" + LOW.ToString() + "-" + HIGH.ToString() + ")";
        //        string strFileName = "Barcode (" + (dsItems.Tables[0].Rows[0]["GRNMasterSystemID"].ToString()) + ") Color(" + dsItems.Tables[0].Rows[0]["SKU"].ToString() + ")" + System.DateTime.Today.ToString("dd-MMM-yyyy") + range + ".pdf";

        //        doc.Save(strFileName + ".pdf", response, HttpReadType.Save);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        doc = null;
        //    }

        //}

    }
}