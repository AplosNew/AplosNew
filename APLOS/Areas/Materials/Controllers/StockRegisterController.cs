#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Materials;
using Library.ViewModel.Materials;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.MaterialManagement;




#endregion using

namespace Aplos.Areas.Materials.Controllers
{
    public class StockRegisterController : BaseController
    {
        #region -- Constructor
        //private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IInventoryReceiveService _inventoryReceiveService;
        private readonly ISqlRepository _sqlRepository;

        private readonly IMaterialMasterAlternativeUOMService _materialMasterAlternativeUOMService;
        private readonly IMaterialMasterProcessRoutingService _materialMasterProcessRoutingService;
        private readonly IMaterialMasterUsageService _materialMasterUsageService;
        private readonly IMaterialMasterAttributeValueService _materialMasterAttributeValueService;
        private readonly IMaterialAttributeValueService _materialValueService;
        private readonly IMaterialMasterCharacteristicsValueService _materialMasterCharacteristicsValueService;
        private readonly IMaterialMasterProcessSetService _materialMasterProcessService;
        private readonly IMaterialMasterMachineProcessService _assetItemProcessService;
        //private readonly IInventoryReceiveService _inventoryReceiveService;

        public StockRegisterController(
              ISqlRepository sqlRepository,
              IInventoryReceiveService inventoryReceiveService
             , IMaterialMasterService materialMasterService
            , IMaterialMasterAlternativeUOMService materialMasterAlternativeUOMService
            , IMaterialMasterProcessRoutingService materialMasterProcessRoutingService
            , IMaterialMasterUsageService materialMasterUsageService
            , IMaterialMasterAttributeValueService materialMasterAttributeValueService
            , IMaterialMasterCharacteristicsValueService materialMasterCharacteristicsValueService
            , IMaterialMasterProcessSetService materialMasterProcessService
            , IMaterialMasterMachineProcessService assetItemProcessService
            , IMaterialAttributeValueService materialValueService
        
            )
        {

            _sqlRepository = sqlRepository;
             _inventoryReceiveService = inventoryReceiveService;
            _materialMasterService = materialMasterService;
            _materialMasterAlternativeUOMService = materialMasterAlternativeUOMService;
            _materialMasterProcessRoutingService = materialMasterProcessRoutingService;
            _materialMasterUsageService = materialMasterUsageService;
            _materialMasterAttributeValueService = materialMasterAttributeValueService;
            _materialMasterCharacteristicsValueService = materialMasterCharacteristicsValueService;
            _materialMasterProcessService = materialMasterProcessService;
            _assetItemProcessService = assetItemProcessService;
            _materialValueService = materialValueService;
    

        }

        #endregion -- Constructor

        #region Pages
       
		public ActionResult StockRegister() 
		{
			return View();
		}
		public ActionResult RequisitionStatus()
		{
			return View();
		}

        [HttpPost, Authorize]
        public ActionResult StockRegisterData(string ToDate, string FromDate, int Days, string Type)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(GetStockRegisterData(identity.PlantId, FromDate, ToDate, Days, Type));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetStockRegisterData(string PlantId, string FromDate, string ToDate, int Days, string Type)
        {
            try
            {
                var tempquery = "";
                var temptype = "";
                if (FromDate == null || FromDate == "")
                { tempquery = " AND convert(Date,IR.GRNDate) <  '" + ToDate + "'"; }
                else
                {
                    tempquery = " AND convert(Date,IR.GRNDate) BETWEEN  '" + FromDate + "' AND '" + ToDate + @"'";
                }
                if(Type == "Regular" || Type == "Irregular") {
                    temptype = " AND x.IsRegular = '" + Type + "'";
                }
                else
                {
                    temptype = "";
                }

                var str = @"SELECT * FROM (SELECT   ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo  
							,IsRegular =case when MM.IsRegular=1 then 'Regular' else 'Irregular' end
							,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,IM.MaterialMasterId
						,MM.UserName MaterialMasterName
						,ART.StandardName ArticleName, ISNULL(FCV.UserName,'') AS SKU1
						,ISNULL(SCV.UserName,'') AS SKU2
						,IR.Id As GRNNo,IRD.Id As GRNROWId,   REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
						,IRD.TransactionQty GRNQty
						,IRD.ShortageQty 
						,TUoM.UserName AS UOM
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2)  GRNMaterialAmount
						,IRD.BaseQty-IRD.ShortageQty-IRD.IssueQty BalanceStock
						,DATEDIFF(day, IR.GRNDate,GETDATE()) AS 'StockInDays'
                        ,IsAsset=CASE WHEN IRD.IsAsset=0 then 'No' else 'Yes' END
						,MS.UserName StorageLocation,'' StorageResponsiblePerson
						,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                        ,p.UserName AS PartyName
						,EI.EmployeeName FirstName						   
						,IR.GateEntryNo
						,IR.DocRefNo
						,IR.AddedBy
                        ,CASE  WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
						,pod.InventoryReceiveId PONo,REPLACE(CONVERT(CHAR(11), po.PODate, 106),' ','-') AS PODate,pod.TransactionQty POQty
						,MRM.Id RequsitionNo,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS  RequisitionDate,MRD.TransactionQty RequisitionQty
						,EMRM.EmployeeName RequisitionAddedBy
						,EMRM1.EmployeeName ReqCheckBy
						,EMRM2.EmployeeName ReqApproveBy
					from TRN.InventoryMaterial AS IM
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					left JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					LEFT jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id	
					LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					left JOIN org.Company AS co  ON co.Id=ir.CompanyId
					left JOIN [SCS].[Currency] AS CU ON Co.BaseCurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor' 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    LEFT JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					LEFT JOIN trn.Voucher V on V.Id=I.VoucherId
                    LEFT JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					LEFT JOIN trn.Voucher V1 on V1.Id=ep.VoucherId
					LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo	
					LEFT JOIN trn.PurchaseOrderDetail pod on pod.Id=IRD.PODetailsId
					LEFT JOIN trn.PurchaseOrder po on po.Id=pod.InventoryReceiveId
					LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.Id=pod.RequisitionDetailId
					LEFT JOIN TRN.MaterialRequsitionMaster MRM ON MRM.Id=MRD.MaterialReqqusitionMasterId
					LEFT JOIN EmployeeInformation EMRM ON EMRM.SystemId=MRM.AddedBy
					LEFT JOIN EmployeeInformation EMRM1 ON EMRM1.SystemId=MRM.CheckedBy
					LEFT JOIN EmployeeInformation EMRM2 ON EMRM2.SystemId=MRM.AuthorizedBy
					WHERE  IR.PlantId='" + PlantId + "' " + tempquery + @"
						AND (isnull(ir.AuthorizedByStatus,'')!='Reject') and   isnull(ir.CheckedByStatus,'')!='Reject'
                        AND IRD.QualityStatus!='Reject'
					AND (IRD.BaseQty-IRD.IssueQty)>0   AND IRD.Id  NOT IN (SELECT ISNULL(InventoryReceiveDetailId,'') FROM [TRN].[CapitalizationMasterDetail]  where InventoryIssueHistoryId IS NULL)
						) x where x.StockInDays >= " + Days + " "+ temptype + "";
                return _sqlRepository.GetDataTable(str);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost, Authorize]
        public ActionResult RequisitionStatusData(string employeeId, string requisitionFromDate,string requisitionToDate, string requisitionStatus)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(GetRequisitionStatusData(employeeId, requisitionFromDate, requisitionToDate, requisitionStatus));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetRequisitionStatusData(string employeeId, string requisitionFromDate, string requisitionToDate, string requisitionStatus)
        {
            try
            {
                var EmpAll = "";
                if (employeeId!=null &&  requisitionStatus== "Regular")
                {
                    EmpAll = "AND RM.ReqEmpId = '" + employeeId + @"' AND RM.RequisitionDate between '" + requisitionFromDate + "' AND '"+ requisitionToDate + "' MM.IsRegular=1";
                }
                else if(employeeId != null && requisitionStatus == "Irregular")
                {
                    EmpAll = "AND RM.ReqEmpId = '" + employeeId + @"' AND RM.RequisitionDate between '" + requisitionFromDate + "' AND '" + requisitionToDate + "' MM.IsRegular=0";
                }
                else if (employeeId != null && requisitionStatus == "All")
                {
                    EmpAll = "AND RM.ReqEmpId = '" + employeeId + @"' AND RM.RequisitionDate between '" + requisitionFromDate + "' AND '" + requisitionToDate + "' ";
                }
                else if (employeeId == null && requisitionStatus == "Regular")
                {
                    EmpAll = "  AND RM.RequisitionDate between '" + requisitionFromDate + "' AND '" + requisitionToDate + "' AND  MM.IsRegular=1";
                }
                else if (employeeId == null && requisitionStatus == "Irregular")
                {
                    EmpAll = "  AND RM.RequisitionDate between '" + requisitionFromDate + "' AND '" + requisitionToDate + "' AND MM.IsRegular=0";
                }
                else
                {
                    EmpAll = "AND RM.RequisitionDate between '" + requisitionFromDate + "' AND '" + requisitionToDate + "'";
                }
				var str = @"SELECT * FROM (
		select EI.EmployeeCode,rm.EntityId,EI.EmployeeName EN,ET.UserName EntityName,D.UserName Department,DV.userName Division,RM.ReqEmpId
		,ReqStatus=case when MM.IsRegular=1 then 'Regular' else 'Irregular' end
		,format(RM.RequisitionDate,'dd-MMM-yyy')RequisitionDate,RM.Id,RMD.Id ROWId,MM.UserName Material,ART.StandardName Article,TUoM.UserName UOM
		,ISNULL(RMD.TransactionQty,0) ReqQty,ISNULL(RMD.TotalAmount,0) ReqAmount
                                            ,ISNULL(POD.POQty,0)POQty
											,isnull(POD.POAmount,0) POAmount
											,BalancePOQty=Case WHEN ISNULL(RMD.TransactionQty,0)-ISNULL(POD.POQty,0)>0 THEN ISNULL(RMD.TransactionQty,0)-ISNULL(POD.POQty,0) ELSE 0 END
											,ISNULL(GRM.GRNQty,0)GRNQty,ISNULL(GRM.GRNAmount,0) GRNAmount
											,BalanceToReceive=ISNULL(POD.POQty,0)-ISNULL(GRM.GRNQty,0)
											,ISNULL(GRM.IssueQty,0) IssueQty
                                            ,0 SalesQty
											--,BalanceToReceive=ISNULL(RMD.TransactionQty,0)-ISNULL(GRM.GRNQty,0)
											,RM.Remarks
                                            from  TRN.MaterialRequsitionMaster RM 
                                            LEFT JOIN TRN.MaterialRequsitionDetails RMD ON RMD.MaterialReqqusitionMasterId=RM.Id
                                            LEFT JOIN (
	                                            SELECT poRD.RequisitionDetailId ,sum(ISNULL(poRD.TransactionQty,0)) POQty
												,sum(ISNULL(pd.TransactionAmount,0)) POAmount
		                                            from TRN.PoRequisitionDetail poRD 
													JOIN TRN.PurchaseOrderDetail pd ON pd.Id=poRD.PODetailId
		                                            group by poRD.RequisitionDetailId
                                            ) POD ON POD.RequisitionDetailId=RMD.Id
											LEFT JOIN (
												SELECT GRM.ReqDetailId,SUM(ISNULL(GRM.TransactionQty,0)) GRNQty 
												,SUM(ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0)) GRNAmount,SUM(ISNULL(II.IssueQty,0)) IssueQty
												FROM TRN.GRNPORequisitionMap GRM 
												LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=GRM.InventoryReceiveDetailId
												LEFT JOIN (SELECT IIH.InventoryReceiveDetailId,SUM(ISNULL(IIH.Qty,0)) IssueQty 
													FROM TRN.InventoryIssueHistory IIH GROUP BY IIH.InventoryReceiveDetailId) II ON II.InventoryReceiveDetailId=IRD.Id
													GROUP BY GRM.ReqDetailId
											) GRM ON GRM.ReqDetailId=RMD.Id
                                            LEFT JOIN MST.MaterialMasterArticle AS ART ON RMD.ArticleId=ART.Id
                                            LEFT JOIN MST.MaterialMaster AS MM ON ART.MaterialMasterId=MM.Id
                                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                                            LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON RMD.TransactionUoMId=TUoM.Id	
											left join EmployeeInformation EI on EI.SystemId=RM.ReqEmpId
											left join ORG.Department D on EI.DepartmentId=D.Id
											left join ORG.Division DV on EI.DivisionId=DV.Id
											left join ORG.Entity ET on ET.Id=RM.EntityId

                                            where RMD.Id is not null " + EmpAll + ") x ";
                return _sqlRepository.GetDataTable(str);

            }
            catch (Exception e)
            {
                throw e;
            }
        }


		#endregion Pages
	}

}