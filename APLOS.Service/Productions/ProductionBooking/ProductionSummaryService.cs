#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Model.Productions.ProductionBooking;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Productions
{
    public class ProductionSummaryService : Service<ProductionSummary>, IProductionSummaryService
    {
        #region Constructor

        private readonly IProductionSummaryDetailService _psds;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<FGInventoryReceive> _FGInventoryReceiveRepository;
        private readonly IRepositoryAsync<ProductionOrderProcessSet> _ProductionOrderProcessSetRepository;

        public ProductionSummaryService(
            IRepositoryAsync<ProductionSummary> ProductionSummaryRepository,
            IRepositoryAsync<FGInventoryReceive> FGInventoryReceiveRepository,
            IRepositoryAsync<ProductionOrderProcessSet> ProductionOrderProcessSetRepository,
            IProductionSummaryDetailService psds,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProductionSummaryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _psds = psds;
            _FGInventoryReceiveRepository = FGInventoryReceiveRepository;
            _ProductionOrderProcessSetRepository = ProductionOrderProcessSetRepository;
        }

        #endregion Constructor

        private DataSet GetIsFinishGoods(string processId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT * FROM [HKP].[EntityProcessTag] WHERE ProcessNature='Packing' AND IsFinishGoods=1 AND ProcessId='" + processId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public IEnumerable<object> Query(string plantId)
        {
            try
            {
                string _sql = @"SELECT PS.Id
		                                ,PS.PlantId
		                                ,PS.SAMUomId
		                                ,PS.NeoclearProcessId
		                                ,PS.BomOrRecipe
		                                ,PS.IsMultipleOrderAllowedInBatch
                                        ,PS.LotNumber
                                FROM TRN.ProductionSummary AS PS
                                WHERE PS.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ProductionSummary), out sID);
            return sID;
        }

        public IEnumerable<object> GetChar1Info(string id, string soid)
        {
            try
            {

                string _sql = @"SELECT
                                    psd.Id,so.id SalesOrderId, 
                                    c1.UserName FirstChar,cv1.UserName FirstCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,fc.Id FCharId,fc.CharacteristicsId Characteristics1Id
									,fc.CharacteristicsValueId Characteristics1ValueId
                                    ,psd.Qty
                                    FROM  TRN.SalesOrder so
                                    LEFT JOIN TRN.MasterOrderItem moi on so.MasterOrderItemId=moi.id
                                    LEFT JOIN MST.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                    --LEFT JOIN [MST].[MaterialMasterCharacteristics] mmc on mmc.MaterialMasterId=mm.id
                                    LEFT JOIN [TRN].[FirstCharacteristics] fc on so.id=fc.SalesOrderId --and fc.CharacteristicsId=mmc.CharacteristicsId

                                    LEFT JOIN HKP.Characteristics c1 on c1.id=fc.CharacteristicsId  
                                    LEFT JOIN HKP.CharacteristicsValue cv1 on cv1.id=fc.CharacteristicsValueId

                                    --transaction tables
									LEFT JOIN (
									SELECT d.Id,d.FCharId
												,d.SCharId,d.TCharId
												,d.Qty
												,p.MaterialMasterId
												,p.ArticleId
												,p.SalesOrderId
												FROM TRN.ProductionSummary p 
												LEFT JOIN TRN.ProductionSummaryDetail d on p.id=d.ProductionSummaryId 

												where p.Id= '" + id + @"'                                                
									) psd on psd.SalesOrderId=so.id and psd.MaterialMasterId=moi.MaterialMasterId and psd.ArticleId=moi.ArticleId
									AND psd.FCharId=fc.Id 
                                    WHERE so.id='" + soid + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetChar1InfobyPrO(string id, string soid)
        {
            try
            {

                string _sql = @"SELECT
                                    psd.Id,so.id SalesOrderId, 
                                    c1.UserName FirstChar,cv1.UserName FirstCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,fc.Id FCharId,fc.CharacteristicsId Characteristics1Id
									,fc.CharacteristicsValueId Characteristics1ValueId
                                    ,psd.Qty
                                    FROM  TRN.SalesOrder so
                                    LEFT JOIN TRN.MasterOrderItem moi on so.MasterOrderItemId=moi.id
                                    LEFT JOIN MST.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                    --LEFT JOIN [MST].[MaterialMasterCharacteristics] mmc on mmc.MaterialMasterId=mm.id
                                    LEFT JOIN [TRN].[FirstCharacteristics] fc on so.id=fc.SalesOrderId --and fc.CharacteristicsId=mmc.CharacteristicsId

                                    LEFT JOIN HKP.Characteristics c1 on c1.id=fc.CharacteristicsId  
                                    LEFT JOIN HKP.CharacteristicsValue cv1 on cv1.id=fc.CharacteristicsValueId

                                    --transaction tables
									LEFT JOIN (
									SELECT d.Id,d.FCharId
												,d.SCharId,d.TCharId
												,d.Qty
												,p.MaterialMasterId
												,p.ArticleId
												,p.SalesOrderId
												FROM TRN.ProductionSummary p 
												LEFT JOIN TRN.ProductionSummaryDetail d on p.id=d.ProductionSummaryId 

												where p.Id= '" + id + @"'                                                
									) psd on psd.SalesOrderId=so.id and psd.MaterialMasterId=moi.MaterialMasterId and psd.ArticleId=moi.ArticleId
									AND psd.FCharId=fc.Id 
                                     WHERE so.id IN (Select SalesOrderId from TRN.ProductionOrderDetail Where ProductionOrderId='" + soid + "')";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetMentorAndRespPersonByWCM(string wcmId)
        {
            try
            {
                var _sql = @"SELECT W.ResponsiblePersonId, R.EmployeeName ResponsiblePersonName,W.MentorId, M.EmployeeName MentorName 
                            FROM [SCS].[WorkCenterMaster] W
                            LEFT JOIN EmployeeInformation R ON W.ResponsiblePersonId=R.SystemId
                            LEFT JOIN EmployeeInformation M ON W.MentorId=M.SystemId
                            WHERE Id='" + wcmId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetCharInfo(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            try
            {
                string _sql = string.Empty;
                string wc1 = string.Empty;
                string wc2 = string.Empty;
                if (string.IsNullOrEmpty(artid))
                {
                    wc1 = " and p.MaterialMasterId='" + mmid + @"' ";
                    wc2 = " and mm.id='" + mmid + @"' ";
                }
                else
                {
                    wc1 = " and p.MaterialMasterId='" + mmid + @"' and p.ArticleId='" + artid + @"'";
                    wc2 = " and mm.id='" + mmid + @"' and moi.ArticleId='" + artid + @"'";
                }

                if (CharCount == "1")
                {
                    _sql = @"select
                                    psd.Id,so.id SalesOrderId, 
                                    c1.UserName FirstChar,cv1.UserName FirstCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,fc.Id FCharId,fc.CharacteristicsId Characteristics1Id
									,fc.CharacteristicsValueId Characteristics1ValueId
                                    ,psd.Characteristics1Qty
                                    from  trn.SalesOrder so
                                    left join trn.MasterOrderItem moi on so.MasterOrderItemId=moi.id
                                    left join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                    --left join [MST].[MaterialMasterCharacteristics] mmc on mmc.MaterialMasterId=mm.id
                                    left join [TRN].[FirstCharacteristics] fc on so.id=fc.SalesOrderId --and fc.CharacteristicsId=mmc.CharacteristicsId

                                    left join hkp.Characteristics c1 on c1.id=fc.CharacteristicsId  
                                    left join hkp.CharacteristicsValue cv1 on cv1.id=fc.CharacteristicsValueId

                                    --transaction tables
									left join (
									select d.Id,d.FCharId
												,d.SCharId,d.TCharId
												,d.Characteristics1Qty
												,d.Characteristics2Qty
												,d.Characteristics3Qty

												,p.MaterialMasterId
												,p.ArticleId
												,p.SalesOrderId

												from trn.ProductionSummary p 
												left join trn.ProductionSummaryDetail d on p.id=d.ProductionSummaryId 

												where p.Id='" + masterid + @"'                                                 
									) psd on psd.SalesOrderId=so.id and psd.MaterialMasterId=moi.MaterialMasterId and psd.ArticleId=moi.ArticleId
									and psd.FCharId=fc.Id 

                                    where so.id='" + soid + "'  " + wc2 + @"
                                    and (isnull(cv1.UserName,'')<>'')";
                }
                else
                {
                    _sql = @"SELECT
                                    psd.Id,so.id SalesOrderId, 
                                    c1.UserName FirstChar,cv1.UserName FirstCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,fc.Id FCharId,fc.CharacteristicsId Characteristics1Id
									,fc.CharacteristicsValueId Characteristics1ValueId
                                    ,psd.Qty

									 ,c2.UserName SecondChar,cv2.UserName SecondCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,sc.Id SCharId,sc.CharacteristicsId Characteristics2Id
									,sc.CharacteristicsValueId Characteristics2ValueId
                                    ,psd.ProductionSummaryId
                                    FROM  TRN.SalesOrder so
                                    LEFT JOIN TRN.MasterOrderItem moi on so.MasterOrderItemId=moi.id
                                    LEFT JOIN MST.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                    --left join [MST].[MaterialMasterCharacteristics] mmc on mmc.MaterialMasterId=mm.id

									--- fc
                                    LEFT JOIN [TRN].[FirstCharacteristics] fc on so.id=fc.SalesOrderId --and fc.CharacteristicsId=mmc.CharacteristicsId
                                    LEFT JOIN HKP.Characteristics c1 on c1.id=fc.CharacteristicsId  
                                    LEFT JOIN HKP.CharacteristicsValue cv1 on cv1.id=fc.CharacteristicsValueId
									--- sc
									LEFT JOIN [TRN].[SecondCharacteristics] sc on so.id=sc.SalesOrderId  and fc.Id=sc.FirstCharacteristicsId
                                    LEFT JOIN HKP.Characteristics c2 on c2.id=sc.CharacteristicsId  
                                    LEFT JOIN HKP.CharacteristicsValue cv2 on cv2.id=sc.CharacteristicsValueId

                                    --transaction tables
									LEFT JOIN (
									SELECT d.Id,d.FCharId
												,d.SCharId,d.TCharId
												,d.Characteristics1Id
												,d.Characteristics1ValueId
												,d.Characteristics2Id
												,d.Characteristics2ValueId
												,d.Characteristics3Id
												,d.Characteristics3ValueId
												,d.Qty
												,p.MaterialMasterId
												,p.ArticleId
												,p.SalesOrderId
                                                ,d.ProductionSummaryId
												from trn.ProductionSummary p 
												LEFT JOIN TRN.ProductionSummaryDetail d on p.id=d.ProductionSummaryId 
												WHERE p.Id='" + masterid + @"'                                                 
									) psd on psd.SalesOrderId=so.id AND psd.MaterialMasterId=moi.MaterialMasterId AND psd.ArticleId=moi.ArticleId
									AND psd.FCharId=fc.Id AND psd.SCharId=sc.Id 
                                    WHERE so.id='" + soid + "' AND fc.CharacteristicsValueId='" + CharacteristicsValueId + @"' " + wc2 + @"
                                    --AND (isnull(cv1.UserName,'')<>'')"
                                    ;
                }
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<object> GetCharInfoByPrO(string masterid, string workdate, string mmid, string soid, string artid, string CharCount, string CharacteristicsValueId)
        {
            try
            {
                string _sql = string.Empty;
                string wc1 = string.Empty;
                string wc2 = string.Empty;
                if (string.IsNullOrEmpty(artid))
                {
                    wc1 = " and p.MaterialMasterId='" + mmid + @"' ";
                    wc2 = " and mm.id='" + mmid + @"' ";
                }
                else
                {
                    wc1 = " and p.MaterialMasterId='" + mmid + @"' and p.ArticleId='" + artid + @"'";
                    wc2 = " and mm.id='" + mmid + @"' and moi.ArticleId='" + artid + @"'";
                }

                if (CharCount == "1")
                {
                    _sql = @"select
                                    psd.Id,so.id SalesOrderId, 
                                    c1.UserName FirstChar,cv1.UserName FirstCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,fc.Id FCharId,fc.CharacteristicsId Characteristics1Id
									,fc.CharacteristicsValueId Characteristics1ValueId
                                    ,psd.Characteristics1Qty
                                    from  trn.SalesOrder so
                                    left join trn.MasterOrderItem moi on so.MasterOrderItemId=moi.id
                                    left join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                    --left join [MST].[MaterialMasterCharacteristics] mmc on mmc.MaterialMasterId=mm.id
                                    left join [TRN].[FirstCharacteristics] fc on so.id=fc.SalesOrderId --and fc.CharacteristicsId=mmc.CharacteristicsId

                                    left join hkp.Characteristics c1 on c1.id=fc.CharacteristicsId  
                                    left join hkp.CharacteristicsValue cv1 on cv1.id=fc.CharacteristicsValueId

                                    --transaction tables
									left join (
									select d.Id,d.FCharId
												,d.SCharId,d.TCharId
												,d.Characteristics1Qty
												,d.Characteristics2Qty
												,d.Characteristics3Qty

												,p.MaterialMasterId
												,p.ArticleId
												,p.SalesOrderId

												from trn.ProductionSummary p 
												left join trn.ProductionSummaryDetail d on p.id=d.ProductionSummaryId 

												where p.Id='" + masterid + @"'                                                 
									) psd on psd.SalesOrderId=so.id and psd.MaterialMasterId=moi.MaterialMasterId and psd.ArticleId=moi.ArticleId
									and psd.FCharId=fc.Id 

                                    where so.id=(Select SalesOrderId from TRN.ProductionOrderDetail Where ProductionOrderId='" + soid + "')  " + wc2 + @"
                                    and (isnull(cv1.UserName,'')<>'')";
                }
                else
                {
                    _sql = @"SELECT
                                    psd.Id,so.id SalesOrderId, 
                                    c1.UserName FirstChar,cv1.UserName FirstCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,fc.Id FCharId,fc.CharacteristicsId Characteristics1Id
									,fc.CharacteristicsValueId Characteristics1ValueId
                                    ,psd.Qty

									 ,c2.UserName SecondChar,cv2.UserName SecondCharValue                                   
                                    --,fc.Qty Characteristics1Qty
									,sc.Id SCharId,sc.CharacteristicsId Characteristics2Id
									,sc.CharacteristicsValueId Characteristics2ValueId
                                    ,psd.ProductionSummaryId
                                    FROM  TRN.SalesOrder so
                                    LEFT JOIN TRN.MasterOrderItem moi on so.MasterOrderItemId=moi.id
                                    LEFT JOIN MST.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                    --left join [MST].[MaterialMasterCharacteristics] mmc on mmc.MaterialMasterId=mm.id

									--- fc
                                    LEFT JOIN [TRN].[FirstCharacteristics] fc on so.id=fc.SalesOrderId --and fc.CharacteristicsId=mmc.CharacteristicsId
                                    LEFT JOIN HKP.Characteristics c1 on c1.id=fc.CharacteristicsId  
                                    LEFT JOIN HKP.CharacteristicsValue cv1 on cv1.id=fc.CharacteristicsValueId
									--- sc
									LEFT JOIN [TRN].[SecondCharacteristics] sc on so.id=sc.SalesOrderId  and fc.Id=sc.FirstCharacteristicsId
                                    LEFT JOIN HKP.Characteristics c2 on c2.id=sc.CharacteristicsId  
                                    LEFT JOIN HKP.CharacteristicsValue cv2 on cv2.id=sc.CharacteristicsValueId

                                    --transaction tables
									LEFT JOIN (
									SELECT d.Id,d.FCharId
												,d.SCharId,d.TCharId
												,d.Characteristics1Id
												,d.Characteristics1ValueId
												,d.Characteristics2Id
												,d.Characteristics2ValueId
												,d.Characteristics3Id
												,d.Characteristics3ValueId
												,d.Qty
												,p.MaterialMasterId
												,p.ArticleId
												,p.SalesOrderId
                                                ,d.ProductionSummaryId
												from trn.ProductionSummary p 
												LEFT JOIN TRN.ProductionSummaryDetail d on p.id=d.ProductionSummaryId 
												WHERE p.Id='" + masterid + @"'                                                 
									) psd on psd.SalesOrderId=so.id AND psd.MaterialMasterId=moi.MaterialMasterId AND psd.ArticleId=moi.ArticleId
									AND psd.FCharId=fc.Id AND psd.SCharId=sc.Id 
                                    WHERE so.id IN (Select SalesOrderId from TRN.ProductionOrderDetail Where ProductionOrderId='" + soid + "') AND fc.CharacteristicsValueId='" + CharacteristicsValueId + @"' " + wc2 + @"
                                    --AND (isnull(cv1.UserName,'')<>'')"
                                    ;
                }
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo(string plantId, string ProcessId, string entityId, string CompanyId, string shiftId)
        {
            var sql = @"SELECT Id,UserName FROM SCS.WorkCenterMaster WHERE ProcessId='" + ProcessId + @"' AND PlantId='" + plantId + "'  AND EntityId='" + entityId + "' AND CompanyId='" + CompanyId + "' AND Id IN(SELECT  WorkCenterMasterId FROM [dbo].[WorkCenterWiseShift] WHERE ShiftDefinationID='"+ shiftId + "') Order by Sequence";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetToWCCbo(string plantId, string ProcessId, string entityId, string CompanyId)
        {
            var sql = @"SELECT Id,UserName FROM SCS.WorkCenterMaster WHERE ProcessId='" + ProcessId + @"' AND PlantId='" + plantId + "'  AND EntityId='" + entityId + "' AND CompanyId='" + CompanyId + "' Order by Sequence";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }
        public IEnumerable<object> GetCboWC(string plantId, string ProcessId, string entityId, string productionDate, string shiftId, string ProductionInChargeId)
        {
            var sql = @"SELECT distinct wc.Id as WorkCenterMasterId,CAST (CASE WHEN pw.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,pw.Id,wc.UserName as WorkCenter,
                        isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"'and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as ProductionOrderId,isnull(pw.LotNumber,(select top 1 LotNumber from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"'and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc)) as LotNumber,M.EmployeeName as Mentor,
                        PI.EmployeeName as ProductionInCharge,PI.SystemId as ProductionInChargeId,
                        isnull(R.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId ='" + shiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePerson,
                        isnull(R.SystemId,(select SystemId from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePersonId,
                        isnull(I.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 InChargeId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId ='" + shiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InCharge,
                        isnull(I.SystemId,(select SystemId from EmployeeInformation where SystemId = (select top 1 InChargeId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId = '" + shiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InChargeId,
                        isnull(C.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 CheckedBy from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))) as CheckedByName,pw.Quantity,isnull(pw.ProductionGrade,'A') as ProductionGrade,pw.Remarks,isnull(SM.SumMinute,0) as SumMin,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty,
                        isnull(CEILING(PQ.Qty),PO.PlannedQty) OrderQty,
						ISNULL(CEILING(PRS.TotalProductionQty), 0) as BookedQty,RM.TargetProductionFP,isnull(PPS.ProductionBookingLevel,(select ProductionBookingLevel from hkp.EntityProcessTag where EntityId='" + entityId + "' and ProcessId='" + ProcessId + @"')) as BookingLevel,pw.SalesOrderId,
(select MA.StandardName from trn.salesorder SO
left outer join trn.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where SO.Id=pw.SalesOrderId) as SOArticle,pw.MasterOrderItemId,(select MA.StandardName from trn.MasterOrderItem MOI
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where MOI.Id=pw.MasterOrderItemId) as MOIArticle,(select MA.StandardName from trn.MasterOrderItem MOI
left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
where MOI.Id=pw.MasterOrderItemId) as ProductCodeArticle,
                                       Article =STUFF((select distinct ','+MA.StandardName from trn.ProductionOrderDetail Pod 
                                                            left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                            where Pod.ProductionOrderId=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
						               SONo=STUFF((select distinct ','+sox.Id from trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                       Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                where Xpod.ProductionOrderId=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                      ProductCode=STUFF((select distinct ','+PM.Code from trn.ProductionOrderDetail Pod
                                                            left outer JOIN trn.SalesOrder SO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                            where Pod.ProductionOrderId=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
						             ProductDetails=STUFF((select distinct ','+PM.UserName from trn.ProductionOrderDetail Pod
                                                            left outer JOIN trn.SalesOrder SO ON pod.SalesOrderId=so.Id
                                                            left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                            left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                            where Pod.ProductionOrderId=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
						             CustomerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from trn.MasterOrder XMOI 	 
								                            INNER JOIN trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') 
                        FROM  SCS.WorkCenterMaster wc 
                        LEFT JOIN TRN.ProductionSummary pw ON pw.WorkCenterMasterId=wc.Id AND pw.ProcessId = '" + ProcessId + @"' 
                        AND  pw.EntityId='" + entityId + @"' AND PW.ProductionDate='" + productionDate + @"'  AND PW.ProductionShiftId='" + shiftId + @"' 
                        LEFT JOIN trn.ProductionOrder AS PO ON PO.ID=isnull(pw.ProductionOrderId,(select top 1 ProductionOrderId from TRN.ProductionSummary where ProcessId = '" + ProcessId + @"' and EntityId='" + entityId + "' and ProductionShiftId='" + shiftId + @"' and WorkCenterMasterId=wc.Id order by AddedDate desc))
						LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + ProcessId + @"'
                        LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = PO.Id
						 LEFT JOIN
                            (SELECT SUM(PS.Quantity) TotalProductionQty, PS.ProductionOrderId
                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + ProcessId + @"'  GROUP BY PS.ProductionOrderId
                            ) AS PRS ON PRS.ProductionOrderId = PO.Id     
                        LEFT JOIN EmployeeInformation R ON PW.ResponsiblePersonId=R.SystemId
                        LEFT JOIN EmployeeInformation M ON PW.MentorId=M.SystemId
                        LEFT JOIN EmployeeInformation C ON PW.CheckedBy=C.SystemId
                        LEFT JOIN EmployeeInformation I ON PW.InChargeId=I.SystemId
                        LEFT JOIN EmployeeInformation PI ON PW.ProductionInChargeId=PI.SystemId
                        LEFT JOIN TRN.RunningMachineSetUpTarget RM ON RM.EntityId='" + entityId + @"' and RM.ProcessId='" + ProcessId + @"' and RM.TargetDate='" + productionDate + @"' and RM.ProductionShiftId='" + shiftId + @"' and RM.WorkCenterMasterId=wc.Id and RM.ProductionOrderId=pw.ProductionOrderId
						LEFT JOIN (select ISNULL(sum(Minute),0) as SumMinute,WorkCenterId, ProductionSummaryId from MachineMasterTransaction MT where MT.ProcessId='" + ProcessId + @"' and MT.EntityId = '" + entityId + @"' AND MT.Date='" + productionDate + @"'  AND MT.ShiftId='" + shiftId + @"'
                        group by WorkCenterId,ProductionSummaryId) SM ON SM.WorkCenterId=wc.Id and SM.ProductionSummaryId=pw.Id
                        where wc.Active=1 and wc.ProcessId = '" + ProcessId + @"' and wc.EntityId = '" + entityId + @"' order by wc.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetWSCWC(string plantId, string ProcessId, string entityId, string Date, string shiftId, string WSMId)
        {
            var sql = @"select B.Id,B.ProcessId,B.EntityId,B.ShiftId,B.Date,B.WorkCenterMasterId,B.WorkCenter,B.WorkStation,B.ResponsiblePerson,B.ResponsiblePersonId,B.InCharge,B.InChargeId,B.Remarks,B.ItemName,B.ColumnInfoId,B.Sequence,B.Column1,B.Column2,B.Column3,B.Column4
into #tempPC from 
 (select A.Id,A.ProcessId,A.EntityId,A.ShiftId,A.Date,A.WorkCenterMasterId,A.WorkCenter,A.WorkStation,A.ResponsiblePerson,A.ResponsiblePersonId,A.InCharge,A.InChargeId,A.Remarks,A.ItemName,A.ColumnInfoId,A.Sequence,A.Column1,A.Column2,A.Column3,A.Column4 from
 (SELECT distinct wcs.Id,wcs.ProcessId,wcs.EntityId,wcs.ShiftId,wcs.Date,wc.Id as WorkCenterMasterId,CAST (CASE WHEN wcs.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,wc.UserName as WorkCenter,wc.NoOfWorkStation as WorkStation,
                        isnull(R.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.WCWorkStationControlSummary where ProcessId = '" + ProcessId +"' and EntityId='"+ entityId +"' and ShiftId ='"+ shiftId +@"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePerson,
                        isnull(R.SystemId,(select SystemId from EmployeeInformation where SystemId = (select top 1 ResponsiblePersonId from TRN.WCWorkStationControlSummary where ProcessId = '"+ ProcessId +"' and EntityId='"+ entityId +"' and ShiftId = '"+ shiftId +@"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as ResponsiblePersonId,
                        isnull(I.EmployeeName,(select EmployeeName from EmployeeInformation where SystemId = (select top 1 InChargeId from TRN.WCWorkStationControlSummary where ProcessId = '"+ ProcessId +"' and EntityId='"+ entityId +"' and ShiftId ='"+ shiftId +@"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InCharge,
                        isnull(I.SystemId,(select SystemId from EmployeeInformation where SystemId = (select top 1 InChargeId from TRN.WCWorkStationControlSummary where ProcessId = '"+ ProcessId +"' and EntityId='"+ entityId +"' and ShiftId = '"+ shiftId + @"' and WorkCenterMasterID=WC.Id order by AddedDate desc))) as InChargeId,
                        wcs.Remarks,CD.ColumnInfoId,wcs.Column1,wcs.Column2,wcs.Column3,wcs.Column4,'0' as ItemName,wc.Sequence

                        FROM  SCS.WorkCenterMaster wc 
                        LEFT JOIN TRN.WCWorkStationControlSummary wcs ON wcs.WorkCenterMasterId=wc.Id AND wcs.ProcessId = '" + ProcessId +@"' 
                        AND  wcs.EntityId='"+ entityId +"' AND wcs.Date='"+ Date +"'  AND wcs.ShiftId='"+ shiftId + @"' 
                        LEFT JOIN EmployeeInformation R ON wcs.ResponsiblePersonId=R.SystemId
                        LEFT JOIN EmployeeInformation I ON wcs.InChargeId=I.SystemId
						LEFT JOIN TRN.ColumnsDetails CD ON CD.WSMId='" + WSMId + @"' and CD.Active=1
                        where wc.Active=1 and wc.ProcessId = '"+ ProcessId +"' and wc.EntityId = '"+ entityId + @"')A
				)B order by B.Sequence 

DECLARE @sql nvarchar(max), @col nvarchar(max)

 SELECT @col = (
 SELECT DISTINCT ',' + QUOTENAME(REPLACE(CONVERT(VARCHAR(40), ColumnInfoId, 113), ' ', '-'))

 FROM #tempPC 
                                FOR XML PATH('')
                            )                             SELECT @sql = N'
 (SELECT *
 FROM #tempPC
                            PIVOT(
 MAX([ItemName]) FOR[ColumnInfoId] IN('+STUFF(@col,1,1,'')+')
 ) as pvt)' 

 EXEC sp_executesql @sql
 drop table #tempPC";
            return _sqlRepository.GetDataCollection(sql);
        }
        public IEnumerable<ComboModel> GetCharacteristicsValueCbo(string soid)
        {
            var sql = @"SELECT C.Id, C.UserName FROM [TRN].[FirstCharacteristics] FC
                        LEFT JOIN hkp.CharacteristicsValue C ON C.Id=FC.CharacteristicsValueId where FC.SalesOrderId='" + soid + "'";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetCharacteristicsValueByPrOCbo(string soid)
        {

            string sql = @"SELECT C.Id, C.UserName FROM [TRN].[FirstCharacteristics] FC
                            LEFT JOIN hkp.CharacteristicsValue C ON C.Id=FC.CharacteristicsValueId 
                            LEFT JOIN TRN.ProductionOrderDetail PD ON PD.SalesOrderId=FC.SalesOrderId
                            where PD.ProductionOrderId='" + soid + "'";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<ComboModel> GetShiftGroupCbo(string plantId)
        {
            var sql = @"SELECT Id,Description UserName FROM MST.CompliedShiftGrouping WHERE PlantId='" + plantId + "'";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public IEnumerable<object> GetSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
            if (productionLevel != "ProductionOrder")
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,so.Id SalesOrderId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') ProductionOrderId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,SO.PlannedQty
	                               	,ISNULL(PRS.TotalProductionQty,0) TotalProductionQty
	                                ,ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else
            {
                string CmdText = @"SELECT PO.Id ProductionOrderId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PO.EntityId='" + entityid + "' AND PS.UserName = 'Running'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
        }

        public IEnumerable<object> GetTotalProductionQty(string WorkCenterMasterId, string ProductionDate)
        {
            try
            {
                var sql = @"SELECT CEILING(SUM(Quantity)) TotalProductionQty  FROM [TRN].[ProductionSummary] WHERE WorkCenterMasterId='" + WorkCenterMasterId + @"'  AND ProductionDate='" + ProductionDate + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetTotalQty(string salesOrderId, string processId)
        {
            try
            {
                var sql = @"SELECT SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty
                                ,ISNULL((SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) -PRS.TotalProductionQty),0) RemainingQty, ISNULL(PRS.TotalProductionQty,0)TotalProductionQty
                                FROM trn.SalesOrder AS so
                                INNER JOIN TRN.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId
	                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId
	                            ) AS PRS ON PRS.SalesOrderId = SO.Id WHERE so.Id ='" + salesOrderId + "' GROUP BY TotalProductionQty";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                //var ob_fromDB=Find(ps.Id);
                var ob_fromDB = GetProductionSummaryList(ps).FirstOrDefault();
                if (ob_fromDB == null)
                {
                    ps.Id = "PS" + GetPK();
                    ps.ModelState = ModelState.Added;
                    AuditService.AddedLog(ps);
                }
                else
                {
                    ps.Id = ob_fromDB.Id;
                    ps.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(ps);
                }
                _psds.Save(ps.Id, psd);
                base.InsertOrUpdateGraph(ps);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, ps.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private DataSet GetProductionPeriodData(DateTime? addedDate)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {

                ExportType = "DATASET",
                CmdText = @"SELECT X.Id, X.StartTime, X.EndTime FROM 
                            (
                            SELECT Id,CONVERT(datetime,(FORMAT(GETDATE(),'dd-MMM-yyyy')+' ' + CONVERT(VARCHAR(5), StartTime, 108))) StartTime,
                                  CONVERT(datetime,(FORMAT(GETDATE(),'dd-MMM-yyyy')+' ' + CONVERT(VARCHAR(5), EndTime, 108))) EndTime
                            FROM HKP.ProductionBookingPeriod
                            ) X WHERE '" + addedDate + @"' between  X.StartTime  AND X.EndTime"
            };
            parameters.order = "X.StartTime";
            return _sqlRepository.GetGridData(parameters).Source;
        }
        public void SaveMaster(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string companyGroupId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var ob_fromDB = Find(ps.Id);
                if (ob_fromDB == null)
                {
                    ps.Id = "P" + GetPK();


                    ps.ModelState = ModelState.Added;
                    AuditService.AddedLog(ps);
                    // ps.AddedDate = DateTime.Now;
                    var pp = GetProductionPeriodData(ps.AddedDate);

                    if (pp.Tables[0].Rows.Count > 1)
                    {
                        throw new CustomException("Production Booking Period can not assign in multiple time.");
                    }
                    else
                    {
                        if (pp.Tables[0].Rows.Count > 0)
                        {
                            ps.ProductionBookingPeriodId = pp.Tables[0].Rows[0]["Id"].ToString();
                        }
                        else
                        {
                            throw new CustomException("There is no Production Booking Period.");
                        }
                    }

                    ps.Quantity = ps.QtyWithoutScan + ps.ScanQty;

                    base.Insert(ps);
                }
                else
                {
                    

                    //ps.Id = ob_fromDB.Id;
                    ob_fromDB.ArticleId = ps.ArticleId;
                    ob_fromDB.MaterialMasterId = ps.MaterialMasterId;
                    ob_fromDB.ProductionGrade = ps.ProductionGrade;
                    ob_fromDB.ProductionBookingPeriodId = ps.ProductionBookingPeriodId;
                    ob_fromDB.UpdatedDate = DateTime.Now;

                    ob_fromDB.ResponsiblePersonId = ps.ResponsiblePersonId;
                    ob_fromDB.InChargeId = ps.InChargeId;
                    ob_fromDB.ProductionInChargeId = ps.ProductionInChargeId;
                    ob_fromDB.MentorId = ps.MentorId;
                    ob_fromDB.ScanQty = ps.ScanQty;
                    ob_fromDB.QtyWithoutScan = ps.QtyWithoutScan;
                    ob_fromDB.Quantity = ps.QtyWithoutScan+ ps.ScanQty;
                    ob_fromDB.ProductionOrderId = ps.ProductionOrderId;
                    ob_fromDB.SalesOrderId = ps.SalesOrderId;
                    ob_fromDB.MasterOrderItemId = ps.MasterOrderItemId;
                    ob_fromDB.ProductLibraryId = ps.ProductLibraryId;

                    ob_fromDB.InTime = ps.InTime;
                    ob_fromDB.OutTime = ps.OutTime;
                    ob_fromDB.LotNumber = ps.LotNumber;
                    ob_fromDB.Remarks = ps.Remarks;
                    ob_fromDB.CheckedBy = ps.CheckedBy;


                    ob_fromDB.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(ob_fromDB);

                    //if (ob_fromDB.AddedDate.AddDays(1) >)
                    //{

                    //}
                    base.Update(ob_fromDB);
                }
                if (psd != null)
                {
                    SaveSecondDetail(psd, ps, companyGroupId, ps.PlantId);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, ps.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveMasterWC(ProductionSummary ps, string companyGroupId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var ob_fromDB = Find(ps.Id);
                if (ob_fromDB == null)
                {
                    ps.Id = "P" + GetPK();


                    ps.ModelState = ModelState.Added;
                    AuditService.AddedLog(ps);
                    // ps.AddedDate = DateTime.Now;
                    var pp = GetProductionPeriodData(ps.AddedDate);

                    if (pp.Tables[0].Rows.Count > 1)
                    {
                        throw new CustomException("Production Booking Period can not assign in multiple time.");
                    }
                    else
                    {
                        if (pp.Tables[0].Rows.Count > 0)
                        {
                            ps.ProductionBookingPeriodId = pp.Tables[0].Rows[0]["Id"].ToString();
                        }
                        else
                        {
                            throw new CustomException("There is no Production Booking Period.");
                        }
                    }

                    ps.Quantity = ps.QtyWithoutScan + ps.ScanQty;

                    base.Insert(ps);
                }
                else
                {


                    //ps.Id = ob_fromDB.Id;
                    ob_fromDB.ArticleId = ps.ArticleId;
                    ob_fromDB.MaterialMasterId = ps.MaterialMasterId;
                    ob_fromDB.ProductionGrade = ps.ProductionGrade;
                    ob_fromDB.ProductionBookingPeriodId = ps.ProductionBookingPeriodId;
                    ob_fromDB.UpdatedDate = DateTime.Now;

                    ob_fromDB.ResponsiblePersonId = ps.ResponsiblePersonId;
                    ob_fromDB.InChargeId = ps.InChargeId;
                    ob_fromDB.ProductionInChargeId = ps.ProductionInChargeId;
                    ob_fromDB.MentorId = ps.MentorId;
                    ob_fromDB.ScanQty = ps.ScanQty;
                    ob_fromDB.QtyWithoutScan = ps.QtyWithoutScan;
                    ob_fromDB.Quantity = ps.QtyWithoutScan + ps.ScanQty;
                    ob_fromDB.ProductionOrderId = ps.ProductionOrderId;
                    ob_fromDB.SalesOrderId = ps.SalesOrderId;
                    ob_fromDB.MasterOrderItemId = ps.MasterOrderItemId;
                    ob_fromDB.ProductLibraryId = ps.ProductLibraryId;

                    ob_fromDB.InTime = ps.InTime;
                    ob_fromDB.OutTime = ps.OutTime;
                    ob_fromDB.LotNumber = ps.LotNumber;
                    ob_fromDB.Remarks = ps.Remarks;
                    ob_fromDB.CheckedBy = ps.CheckedBy;


                    ob_fromDB.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(ob_fromDB);

                    //if (ob_fromDB.AddedDate.AddDays(1) >)
                    //{

                    //}
                    base.Update(ob_fromDB);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, ps.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        public void SaveDetentionWC(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "MachineMasterTransaction";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        if (item["DetentionId"] == null)
                        {

                            throw new CustomException("Detention should not be blank!");
                        }
                        else
                        {
                            //if (item["FromTime"] == null)
                            //{
                            //    throw new CustomException("From time is required!");

                            //}
                            //else
                            //{
                            //    if (item["ToTime"] == null)
                            //    {
                            //        throw new CustomException("To Time is required!");
                            //    }
                            //    else
                            //    {
                                    if (dv.Count == 0)
                                    {
                                        //DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                        //DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                        //DateTime NextDayDate = date2.AddDays(1);
                                        //TimeSpan ts = date2 - date1;
                                        //TimeSpan Nd = NextDayDate - date1;
                                        //int minutes = (int)ts.TotalMinutes;

                                        //if (minutes >= 720 || minutes < 0)
                                        //{
                                        //    item["ToTime"] = NextDayDate;
                                        //    item["Minute"] = Nd.TotalMinutes;
                                        //}
                                        //else
                                        //{
                                        //    item["ToTime"] = date2;
                                        //    item["Minute"] = ts.TotalMinutes;
                                        //}

                                        item["Id"] = GetPK();
                                        AddNewRow(dsProdBooked.Tables[0], item);
                                    }
                                    else
                                    {

                                        DataRow drpb = dv[0].Row;
                                        //DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                        //DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                        //DateTime NextDayDate = date2.AddDays(1);
                                        //TimeSpan ts = date2 - date1;
                                        //TimeSpan Nd = NextDayDate - date1;
                                        //int minutes = (int)ts.TotalMinutes;

                                        //if (minutes >= 720 || minutes < 0)
                                        //{
                                        //    item["ToTime"] = NextDayDate;
                                        //    item["Minute"] = Nd.TotalMinutes;
                                        //}
                                        //else
                                        //{
                                        //    item["ToTime"] = date2;
                                        //    item["Minute"] = ts.TotalMinutes;
                                        //}
                                        EditRow(drpb, item);
                                    }
                                    clsStaticInfo obj = new clsStaticInfo();
                                    obj.SaveDataSets(dsProdBooked);
                                }
                        //    }
                        //}
                    }
                }
                else
                {
                    throw new CustomException("Please enter atleast one row and proceed!");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
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


        public void SaveInOutMaster(ProductionSummary ps, IEnumerable<ProductionSummaryDetail> psd, string companyGroupId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var ob_fromDB = Find(ps.Id);
                if (ob_fromDB == null)
                {
                    //ps.Id = "PS" + GetPK();
                    //ps.AddedDate = DateTime.Now;
                    //if (string.IsNullOrEmpty(ps.ProductionBookingPeriodId))
                    //{
                    //    var pp = GetProductionPeriodData(ps.AddedDate);

                    //    if (pp.Tables[0].Rows.Count > 0)
                    //    {
                    //        ps.ProductionBookingPeriodId = pp.Tables[0].Rows[0]["Id"].ToString();
                    //    } 
                    //}

                    //ps.ModelState = ModelState.Added;
                    //AuditService.AddedLog(ps);
                    //base.Insert(ps);
                    ps.Id = "PS" + GetPK();
                    ps.ModelState = ModelState.Added;
                    AuditService.AddedLog(ps);
                    var pp = GetProductionPeriodData(ps.AddedDate);

                    //if (pp.Tables[0].Rows.Count > 1)
                    //{
                    //    throw new CustomException("Production Booking Period can not assign in multiple time.");
                    //}
                    //else
                    //{
                    //    if (pp.Tables[0].Rows.Count > 0)
                    //    {
                    //        ps.ProductionBookingPeriodId = pp.Tables[0].Rows[0]["Id"].ToString();
                    //    }
                    //    else
                    //    {
                    //        throw new CustomException("There is no Production Booking Period.");
                    //    }
                    //}
                    if (pp.Tables[0].Rows.Count > 0)
                    {
                        ps.ProductionBookingPeriodId = pp.Tables[0].Rows[0]["Id"].ToString();
                    }
                    else
                    {
                        throw new CustomException("There is no Production Booking Period.");
                    }
                    ps.Quantity = ps.QtyWithoutScan + ps.ScanQty;
                    base.Insert(ps);
                }
                else
                {
                    //ps.Id = ob_fromDB.Id;
                    ob_fromDB.ArticleId = ps.ArticleId;
                    ob_fromDB.MaterialMasterId = ps.MaterialMasterId;
                    ob_fromDB.ProductionGrade = ps.ProductionGrade;
                    ob_fromDB.ProductionBookingPeriodId = ps.ProductionBookingPeriodId;
                    ob_fromDB.UpdatedDate = DateTime.Now;

                    ob_fromDB.ResponsiblePersonId = ps.ResponsiblePersonId;
                    ob_fromDB.MentorId = ps.MentorId;
                    ob_fromDB.ScanQty = ps.ScanQty;
                    ob_fromDB.QtyWithoutScan = ps.QtyWithoutScan;
                    ob_fromDB.Quantity = ps.QtyWithoutScan + ps.ScanQty;
                    ob_fromDB.ProductionOrderId = ps.ProductionOrderId;
                    ob_fromDB.SalesOrderId = ps.SalesOrderId;
                    ob_fromDB.MasterOrderItemId = ps.MasterOrderItemId;
                    ob_fromDB.ProductLibraryId = ps.ProductLibraryId;

                    ob_fromDB.InTime = ps.InTime;
                    ob_fromDB.OutTime = ps.OutTime;

                    ob_fromDB.ConsumeHour = ps.ConsumeHour;
                    ob_fromDB.ManPower = ps.ManPower;
                    ob_fromDB.Remarks = ps.Remarks;
                    ob_fromDB.LotNumber = ps.LotNumber;

                    ob_fromDB.WorkCenterMasterId = ps.WorkCenterMasterId;
                    ob_fromDB.ToWorkCenterMasterId = ps.ToWorkCenterMasterId;
                    ob_fromDB.FromSFGInventoryId = ps.FromSFGInventoryId;
                    ob_fromDB.ToSFGInventoryId = ps.ToSFGInventoryId;
                    ob_fromDB.ToProcessId = ps.ToProcessId;
                    ob_fromDB.PackingConfirmationId = ps.PackingConfirmationId;
                    ob_fromDB.ToEntityId = ps.ToEntityId;

                    ob_fromDB.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(ob_fromDB);
                    base.Update(ob_fromDB);
                }
                if (psd != null)
                {
                    SaveSecondDetail(psd, ps, companyGroupId, ps.PlantId);
                }

                if (!string.IsNullOrEmpty(ps.ProcessId))
                {
                    var productionOrderProcessSet = _ProductionOrderProcessSetRepository.Query(r => r.ProductionOrderId == ps.ProductionOrderId && r.ProcessId == ps.ProcessId).Select().FirstOrDefault();
                    if (productionOrderProcessSet == null)
                    {
                        throw new CustomException("Production Order ProcessSet not define.");
                    }
                    if (productionOrderProcessSet.StartDate == null)
                    {
                        productionOrderProcessSet.StartDate = DateTime.Now;
                        AuditService.UpdatedLog(productionOrderProcessSet);
                        _ProductionOrderProcessSetRepository.Update(productionOrderProcessSet);
                    }

                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, ps.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveDetail(string psid, IEnumerable<ProductionSummaryDetail> psd)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                //var ob_fromDB=Find(ps.Id);
                //var ob_fromDB = GetProductionSummaryList(ps).FirstOrDefault();
                //if (ob_fromDB == null)
                //{
                //    ps.Id = "PS" + GetPK();
                //    ps.ModelState = ModelState.Added;
                //    AuditService.AddedLog(ps);
                //}
                //else
                //{
                //    ps.Id = ob_fromDB.Id;
                //    ps.ModelState = ModelState.Modified;
                //    AuditService.UpdatedLog(ps);
                //}
                _psds.Save(psid, psd);
                //base.InsertOrUpdateGraph(ps);

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void SaveSecondDetail(IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary, string companyGroupId, string plantId)
        {
            try
            {
                var fg = GetIsFinishGoods(productionSummary.ProcessId);

                _psds.InsertSecondCharacteristic(psd, productionSummary);

                if (fg.Tables[0].Rows.Count > 0)
                {
                    SaveFGInventoryReceiveData(psd, productionSummary, companyGroupId, plantId);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex);
            }
            finally
            {
            }
        }

        private void SaveFGInventoryReceiveData(IEnumerable<ProductionSummaryDetail> psd, ProductionSummary productionSummary, string companyGroupId, string plantId)
        {
            try
            {
                if (psd != null)
                {
                    foreach (var item in psd)
                    {
                        var fgdata = _FGInventoryReceiveRepository.Query(t => t.ProductionSummaryDetailId == item.Id).Select().FirstOrDefault();
                        if (item.Qty.ToString() != null && item.Qty != 0)
                        {
                            if (fgdata == null)
                            {
                                var fg = new FGInventoryReceive();
                                fg.Id = item.Id + "01";
                                fg.CompanyGroupId = companyGroupId;
                                fg.PlantId = plantId;
                                fg.EntityId = productionSummary.EntityId;
                                fg.ProductionSummaryDetailId = item.Id;
                                fg.MaterialMasterId = productionSummary.MaterialMasterId;
                                fg.ArticleId = productionSummary.ArticleId;
                                fg.FirstCharacteristicsId = item.FCharId;
                                fg.FirstCharacteristicsValueId = item.Characteristics1ValueId;
                                fg.SecondCharacteristicsId = item.SCharId;
                                fg.SecondCharacteristicsValueId = item.Characteristics2ValueId;
                                fg.Qty = item.Qty;

                                fg.ModelState = ModelState.Added;
                                AuditService.AddedLog(fg);
                                _FGInventoryReceiveRepository.InsertOrUpdateGraph(fg);

                            }
                            else
                            {
                                fgdata.Qty = item.Qty;
                                item.ModelState = ModelState.Modified;
                                AuditService.UpdatedLog(fgdata);
                                _FGInventoryReceiveRepository.Update(fgdata);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<ProductionSummary> GetProductionSummaryList(ProductionSummary ps)
        {
            //getbulletindetaillist
            try
            {
                string _sql = "select * from trn.ProductionSummary where ProductionDate='" + ps.ProductionDate + "' and SalesOrderId='" + ps.SalesOrderId + "' and MaterialMasterId='" + ps.MaterialMasterId + "' and ArticleId='" + ps.ArticleId + "'";
                return _sqlRepository.GetModelCollection<ProductionSummary>(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteDetail(string masterid)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                ProductionSummary entity = base.Find(masterid);
                _FGInventoryReceiveRepository.ExecuteSqlCommand(@"Delete from TRN.FGInventoryReceive Where ProductionSummaryDetailId IN (Select D.Id from TRN.ProductionSummaryDetail  D 
                LEFT JOIN TRN.ProductionSummary P ON P.Id=D.ProductionSummaryId Where P.Id='" + masterid + "')");
                _ProductionOrderProcessSetRepository.ExecuteSqlCommand(@"Delete from dbo.ProductionSummaryParameterValue Where ProductionSummaryId='" + masterid + "'");
                _psds.DeleteDetail(masterid);
                base.Delete(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> GetEntity(string CompanyId, string PlantId)
        {
            try
            {
                var _sql = @"SELECT distinct E.Id as Value,E.UserName  AS Text FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            WHERE E.CompanyId='" + CompanyId + "' and E.PlantId='" + PlantId + "' AND ECC.IsProductionEntity=1 AND E.[Active]=1";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetProcess(string entityId)
        {
            try
            {
                //  var _sql = @"SELECT Id as Value,UserName AS Text FROM HKP.Process where CompanyGroupId='"+ CompanyGroupId + "' ";
                var _sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text],EP.ProductionBookingLevel FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId=P.Id WHERE EP.EntityId='" + entityId + "'";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetListAPIforProduction(string ProdnDate, string EntityId)
        {
            try
            {
                var _sql = @"select ps.*,p.UserName as Plant,E.UserName as Entity,Pr.UserName as Process,Wc.UserName as WorkCenter,csg.Description as ProductionShift from TRN.ProductionSummary ps
                                                                    left join ORG.Plant p on ps.PlantId=p.Id
                                                                    left join ORG.Entity E on ps.EntityId=E.Id
                                                                    left join HKP.Process pr on ps.ProcessId=pr.Id
                                                                    left join SCS.WorkCenterMaster wc on ps.WorkCenterMasterId=wc.Id
                                                                    left join MST.CompliedShiftGrouping csg on ps.ProductionShiftId=csg.Id
                                                                    where isnull(ps.ProductionDate,'') = '" + ProdnDate + "' and isnull(ps.EntityId,'') = '" + EntityId + "' ";




                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetListAPIforProduction(string ProdnDate, string EntityId, string ProcessId, string ShiftId)
        {
            try
            {
                var _sql = @"select ps.WorkCenterMasterId,SUM(ps.Quantity)Quantity,ps.ProductionDate,p.UserName as Plant,ps.ProductionShiftId,ps.PlantId,ps.EntityId,ps.ProcessId,E.UserName as Entity,
                             Pr.UserName as Process,Wc.UserName as WorkCenter,csg.Description as ProductionShift
				             from TRN.ProductionSummary ps
                             left join ORG.Plant p on ps.PlantId=p.Id
                             left join ORG.Entity E on ps.EntityId=E.Id
                             left join HKP.Process pr on ps.ProcessId=pr.Id
                             left join SCS.WorkCenterMaster wc on ps.WorkCenterMasterId=wc.Id
                             left join MST.CompliedShiftGrouping csg on ps.ProductionShiftId=csg.Id
                             where isnull(ps.ProductionDate,'') = '" + ProdnDate + @"' and isnull(ps.EntityId,'') = '" + EntityId + @"' and isnull(ps.ProcessId,'')='" + ProcessId + @"' and isnull(ps.ProductionShiftId,'')='" + ShiftId + @"' 
		                     GROUP BY WorkCenterMasterId,
                             p.UserName,E.UserName,Pr.UserName,wc.UserName,ps.PlantId,csg.Description,ps.EntityId,
                             ps.ProcessId,ps.WorkCenterMasterId,ps.ProductionShiftId,ps.ProductionDate";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<ProductionSummary> GetListAPIforProduction(string ProdnDate)
        {

            string strSql = @"select ps.*,p.UserName as Plant,E.UserName as Entity,Pr.UserName as Process,Wc.UserName as WorkCenter,csg.Description as ProductionShift from TRN.ProductionSummary ps
                                                                    left join ORG.Plant p on ps.PlantId=p.Id
																	left join ORG.Entity E on ps.EntityId=E.Id
																	left join HKP.Process pr on ps.ProcessId=pr.Id
																	left join SCS.WorkCenterMaster wc on ps.WorkCenterMasterId=wc.Id
																	left join MST.CompliedShiftGrouping csg on ps.ProductionShiftId=csg.Id                                                 
                                                       where isnull(ps.ProductionDate,'') = '" + ProdnDate + "' ";



            return _sqlRepository.GetModelCollection<ProductionSummary>(strSql, null);
        }

        public IEnumerable<object> GetLineItemGridSFG(string EntityId, string ProcessId, string ProductionDate, string ProductionShiftId, string WorkCenterMasterId, string ProductionLevel)
        {
            try
            {
                string wc = string.Empty;
                if (WorkCenterMasterId != "undefined" && WorkCenterMasterId != "null")
                {
                    wc = @"AND P.WorkCenterMasterId='" + WorkCenterMasterId + @"'";
                }
                if (ProductionLevel != "ProductionOrder")
                {
                    string _sql = @"select p.Id,mo.MasterOrderNo
								,moi.Id MOrderLineNo
								,so.Id SalesOrderId
                                
                                ,PO.PONumber
                                  ,mm.UserName MaterialMaster, mma.StandardName Article
								  ,b.UserName Customer
                                 --,so.ConfirmDate,so.DeliveryDate
                                 ,Replace(CONVERT(VARCHAR(11), so.ConfirmDate, 106), ' ', '-') ConfirmDate
								 ,Replace(CONVERT(VARCHAR(11), so.DeliveryDate, 106), ' ', '-') DeliveryDate
								 ,mo.TotalQty MOQty
								 ,moi.TotalQty MOIQty
                                 ,so.Qty SOQty,p.Quantity ,p.ProductionBookingPeriodId,p.ProductionGrade 
								,u.UserName UOM
								,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP]
								,mma.Id ArticleId,mm.Id MaterialMasterId,mmc.CharCount, p.PlantID,p.WorkCenterMasterId,EP.ProductionBookingLevel
                                ,PBP.UserName ProductionBookingPeriod,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName
                                ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower, P.CheckedBy,C.EmployeeName CheckedByName
                                ,P.ToWorkCenterMasterId,P.FromSFGInventoryId,P.ToSFGInventoryId,P.ToProcessId,P.Remarks,P.WorkCenterMasterId
                                 FROM [TRN].[ProductionSummary] p
								 LEFT JOIN trn.SalesOrder so on so.Id=p.SalesOrderId
                                 LEFT JOIN trn.[MasterOrderItem] moi on moi.id=so.MasterOrderItemId
                                 LEFT JOIN trn.MasterOrder mo on mo.id=moi.MasterOrderId
                                 LEFT JOIN hkp.Party b on b.id=mo.PartyId
								 LEFT JOIN scs.UnitOfMeasurement u on u.id=mo.TotalQtyUOMId
                                 LEFT JOIN mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                 LEFT JOIN mst.MaterialMasterArticle mma on mma.id=moi.ArticleId
                                 LEFT JOIN (
											SELECT count(Id) CharCount,MaterialMasterId from [MST].[MaterialMasterCharacteristics] group by  MaterialMasterId
											) mmc on mmc.MaterialMasterId=mm.id
                                 LEFT JOIN [HKP].[EntityProcessTag] EP ON EP.ProcessId=P.ProcessId and EP.EntityId=P.EntityId
                                 LEFT JOIN [HKP].[ProductionBookingPeriod] PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN [TRN].[CustomerPO] PO ON PO.Id=SO.CustomerPOId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId
                                 WHERE p.EntityId='" + EntityId + @"' 
								 and p.ProcessId='" + ProcessId + @"' 
								 and p.ProductionShiftId='" + ProductionShiftId + @"'  
								 and p.ProductionDate='" + ProductionDate + @"' " + wc + " ";

                    return _sqlRepository.GetDataCollection(_sql, null);

                }
                else
                {
                    string _sql = @"SELECT P.Id,P.ProductionOrderId,FORMAT(P.ProductionDate,'dd-MMM-yyyy') ProductionDate, P.ProductionGrade, P.Quantity, PBP.UserName ProductionBookingPeriod
                                 ,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName, P.CheckedBy,C.EmployeeName CheckedByName
                                 ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower
                                 ,P.ToWorkCenterMasterId,P.FromSFGInventoryId,P.ToSFGInventoryId,P.ToProcessId,P.Remarks,P.WorkCenterMasterId
                                 FROM TRN.ProductionSummary P
                                 LEFT JOIN HKP.ProductionBookingPeriod PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId
                                 WHERE P.EntityId='" + EntityId + @"' 
								 and P.ProcessId='" + ProcessId + @"' 
								 and P.ProductionShiftId='" + ProductionShiftId + @"'  
								 and P.ProductionDate='" + ProductionDate + @"'  " + wc + " ";
                    return _sqlRepository.GetDataCollection(_sql, null);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public IEnumerable<ComboModel> GetCbo(string plantId, string ProcessId)
        {
            var sql = @"SELECT Id,UserName FROM SCS.WorkCenterMaster WHERE ProcessId='" + ProcessId + @"' AND PlantId='" + plantId + "' Order by Sequence";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }

        public string Create(IEnumerable<ProductionSummary> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "TRN.ProductionSummary";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<ProductionSummary> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                string _Id = "";

                foreach (ProductionSummary item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0 && items[0].Id == null)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "PS" + _Id;
                        dr["PlantId"] = item.PlantId;
                        dr["EntityId"] = item.EntityId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["WorkCenterMasterId"] = item.WorkCenterMasterId;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["ProductionGrade"] = item.ProductionGrade;
                        dr["Quantity"] = item.Quantity;
                        dr["ProductionShiftId"] = item.ProductionShiftId;
                        dr["ProductionBookingPeriodId"] = item.ProductionBookingPeriodId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["MentorId"] = item.MentorId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["InTime"] = item.InTime;
                        dr["OutTime"] = item.OutTime;
                        dr["ConsumeHour"] = item.ConsumeHour;
                        dr["ManPower"] = item.ManPower;
                        dr["CheckedBy"] = item.CheckedBy;
                        dr["Remarks"] = item.Remarks;
                        dr["LotNumber"] = item.LotNumber;
                        dr["ToProcessId"] = item.ToProcessId;
                        dr["ToWorkCenterMasterId"] = item.ToWorkCenterMasterId;
                        dr["FromSFGInventoryId"] = item.FromSFGInventoryId;
                        dr["ToSFGInventoryId"] = item.ToSFGInventoryId;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["PlantId"] = item.PlantId;
                        dr["EntityId"] = item.EntityId;
                        dr["ProcessId"] = item.ProcessId;
                        dr["SalesOrderId"] = item.SalesOrderId;
                        dr["MaterialMasterId"] = item.MaterialMasterId;
                        dr["ArticleId"] = item.ArticleId;
                        dr["WorkCenterMasterId"] = item.WorkCenterMasterId;
                        dr["ProductionDate"] = item.ProductionDate;
                        dr["ProductionGrade"] = item.ProductionGrade;
                        dr["Quantity"] = item.Quantity;
                        dr["ProductionShiftId"] = item.ProductionShiftId;
                        dr["ProductionBookingPeriodId"] = item.ProductionBookingPeriodId;
                        dr["ProductionOrderId"] = item.ProductionOrderId;
                        dr["MentorId"] = item.MentorId;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["InTime"] = item.InTime;
                        dr["OutTime"] = item.OutTime;
                        dr["ConsumeHour"] = item.ConsumeHour;
                        dr["ManPower"] = item.ManPower;
                        dr["CheckedBy"] = item.CheckedBy;
                        dr["Remarks"] = item.Remarks;
                        dr["LotNumber"] = item.LotNumber;
                        dr["ToProcessId"] = item.ToProcessId;
                        dr["ToWorkCenterMasterId"] = item.ToWorkCenterMasterId;
                        dr["FromSFGInventoryId"] = item.FromSFGInventoryId;
                        dr["ToSFGInventoryId"] = item.ToSFGInventoryId;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = item.UpdatedFromIP;

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

        public string Delete(IEnumerable<ProductionSummary> DataToDelete)
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
                        objCon.OpenDataSetThroughAdapter("select * from TRN.ProductionSummaryDetail where ProductionSummaryId= '" + item.Id + "' ", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            objCon.ExecuteNonQueryWrapper("Delete FROM TRN.ProductionSummaryDetail WHERE ProductionSummaryId='" + item.Id + "'", true, "1");
                        }
                    }


                    objCon.ExecuteNonQueryWrapper("Delete FROM TRN.ProductionSummary WHERE id='" + item.Id + "'", true, "1");
                }


                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                return ex.ToString();
                //throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
            return "";
        }//end of function

        public IEnumerable<object> GetDetailProductionList(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkCenterId, string ProductionOrderId)
        {
            string sql = "";
            try
            {
                sql = @"select ps.Id, ps.addedby, CAST(ps.AddedDate as time) Time,Buyer =STUFF((select distinct ','+XB.UserName from
                                    trn.SalesOrder XSO                                        
                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                        LEFT JOIN TRN.ProductionOrder po on po.Id=Xpod.ProductionOrderId
                                        left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                        left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                        left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId

                                            where po.Id=" + ProductionOrderId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),                   
                                    ps.Quantity,(emp.EmployeeName) ResponsiblePerson, ps.SalesOrderId,ps.ResponsiblePersonId, ps.ProductionOrderId,ps.Quantity,p.UserName as Plant,E.UserName as Entity,
                                    Pr.UserName as Process,
                                    Wc.UserName as WorkCenter,
                                    csg.Description as ProductionShift from TRN.ProductionSummary ps
                                                                    left join ORG.Plant p on ps.PlantId=p.Id
                                                                    left join ORG.Entity E on ps.EntityId=E.Id
                                                                    left join HKP.Process pr on ps.ProcessId=pr.Id
                                                                    left join SCS.WorkCenterMaster wc on ps.WorkCenterMasterId=wc.Id
                                                                    left join MST.CompliedShiftGrouping csg on ps.ProductionShiftId=csg.Id
                                                                    left join dbo.EmployeeInformation emp on ps.ResponsiblePersonId=emp.SystemId                      
                            where isnull(ps.ProductionDate, '') = '" + ProdnDate + "'and isnull(ps.EntityId,'') = '" + EntityId + "' and isnull(ps.ProcessId,'')= '" + ProcessId + "' and isnull(ps.ProductionShiftId,'')= '" + ShiftId + "'and isnull(ps.WorkCenterMasterId,'')='" + WkCenterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





    }
}