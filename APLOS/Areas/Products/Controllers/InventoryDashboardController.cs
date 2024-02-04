#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Expenses;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.ViewModel.Accounts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.XlsIO;

#endregion Using

namespace Aplos.Areas.Products.Controllers
{
	public class InventoryDashboardController : BaseController
	{
		string fromDateNew = "";
		string toDateNew = "";

		private readonly IInventoryDashboardService _inventoryDashboardService;
		private readonly ISqlRepository _sqlRepository;
		public InventoryDashboardController(IInventoryDashboardService inventoryDashboardService
			, ISqlRepository sqlRepository)
		{
			_inventoryDashboardService = inventoryDashboardService;
			_sqlRepository = sqlRepository;
		}

		public ActionResult Aplos()
		{
			return View();
		}

		public ActionResult InventoryStatus()
		{
			return View();
		}
		[Authorize]
		public ActionResult InventoryStatusD()
		{
			return View();
		}

		public ActionResult InventoryDashboardStatus()
		{
			return View();
		}

		public ActionResult MaterialAgeing()
		{
			return View();
		}

		[HttpGet, Authorize]
		public ActionResult GetCompanyGroupInformation()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.GetCompanyGroupInformation(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetCompanyInformation()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.GetCompanyInformation(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetCompanyPlantInformation()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.GetCompanyPlantInformation(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetVoucherLatestDate(string dateType, string itemType)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.GetVoucherLatestDate(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, dateType, itemType), JsonRequestBehavior.AllowGet);
		}
		[HttpPost, Authorize]
		public ActionResult OrgStructureList(string companyGroupId, string companyId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.OrgStructureList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}
		
		[HttpPost, Authorize]
		public ActionResult DelayList(string companyGroupId, string companyId, string factDate, string fromDate, string toDate, string groupName, string queryString, string queryStringProcess)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.ExpenseList(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate, groupName, queryString, queryStringProcess), JsonRequestBehavior.AllowGet);
		}
		[HttpPost, Authorize]//ExpenseListGraph 
		public ActionResult DelayListGraph(string companyGroupId, string companyId, string factDate, string fromDate, string toDate, string groupName, string queryString, string queryStringProcess)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.ExpenseListGraph(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate, groupName, queryString, queryStringProcess), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult InventoryStatusDashboard(string companyGroupId, string companyId, string factDate, string fromDate, string toDate, string groupName, bool ValueOrNumber, string queryString, string queryStringProcess)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.InventoryStatusDashboard(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate, groupName, ValueOrNumber, queryString, queryStringProcess), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public JsonResult MaterialAgeingStatusDashboard(string companyGroupId, string companyId, string factDate, string fromDate, string toDate, string groupName, bool ValueOrNumber, string queryString, string queryStringProcess, bool IsAsset)
		{
			string paramters = "";
			if (queryString != "")
			{
				if (paramters == "")
					paramters += "isnull(CompanyId,'') in(" + queryString + ")";
				else
					paramters += " AND isnull(CompanyId,'') in(" + queryString + ")";
			}
			if (queryStringProcess != "")
			{
				if (paramters == "")
					paramters += " isnull(PlantId,'') in(" + queryStringProcess + ")";
				else
					paramters += " AND isnull(PlantId,'') in(" + queryStringProcess + ")";
			}

			var sql = "";
			try
			{

				var ValueOrNumber1 = 0;
				if (ValueOrNumber == false)
				{
					ValueOrNumber1 = 0;
				}
				else
				{
					ValueOrNumber1 = 1;
				}
				var IsAsset1 = 0;
				if (IsAsset == false)
				{
					IsAsset1 = 0;
				}
				else
				{
					IsAsset1 = 1;
				}

				//if (groupName == "groupName")
				if (string.IsNullOrEmpty(queryString) && string.IsNullOrEmpty(queryStringProcess))
				{
					sql = @"select CompanyId
							,CompanyName
							,MaterialTypeId,MaterialType
							,ThirtyDaysCount= CASE WHEN CAST(sum(ThirtyDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(ThirtyDaysCount) AS INT) END
							,Total30Value=CASE WHEN CAST(Sum(Total30Value) AS INT) =0 THEN NULL ELSE CAST(Sum(Total30Value) AS INT) END
							,FourtyfiveDaysCount=CASE WHEN CAST(sum(FourtyfiveDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(FourtyfiveDaysCount) AS INT) END
							,Total45Value=CASE WHEN CAST(Sum(Total45Value) AS INT)=0 THEN NULL ELSE CAST(Sum(Total45Value) AS INT) END
							,SixtyDaysCount= CASE WHEN CAST(sum(SixtyDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(SixtyDaysCount) AS INT) END 
							,Total60Value=CASE WHEN CAST(sum(Total60Value) AS INT)=0 THEN NULL ELSE CAST(sum(Total60Value) AS INT) END
							,HundredtwentyDaysCount =CASE WHEN CAST(sum(HundredtwentyDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(HundredtwentyDaysCount) AS INT) END
							,Total120Value= CASE WHEN CAST(sum(Total120Value) AS INT) =0 THEN NULL ELSE CAST(sum(Total120Value) AS INT)  END
							,ThreeHundredSixtyfiveDaysCount=CASE WHEN CAST(sum(ThreeHundredSixtyfiveDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(ThreeHundredSixtyfiveDaysCount) AS INT) END
							,Total365Value=CASE WHEN CAST(sum(Total365Value) AS INT)=0 THEN NULL ELSE CAST(sum(Total365Value) AS INT) END
							,Transaction365QtyGrt=CASE WHEN CAST(sum(Transaction365QtyGrt) AS INT)=0 THEN NULL else CAST(sum(Transaction365QtyGrt) AS INT) END 
							--,Total366Value=CASE WHEN CAST(sum(Total366Value) AS INT) =0 THEN NULL ELSE CAST(sum(Total366Value) AS INT) END 
							,Total366Value=CASE WHEN CAST(sum(Total366Value) AS bigint) =0 THEN NULL ELSE CAST(sum(Total366Value) AS bigint) END 

							FROM
							(
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,sum(IRD.TransactionQty) ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total30Value
								,0 Total45Value
								,0 Total60Value
								,0 Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 0 AND 30
								AND MT.UserName <> ''      AND MM.IsRegular='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName
							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,sum(IRD.TransactionQty) FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total45Value
								,0 Total60Value
								,0 Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 31 AND 45
								AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName		
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,sum(IRD.TransactionQty) SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total60Value
								,0 Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 46 AND 60
								AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,sum(IRD.TransactionQty) HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,0 Total60Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 61 AND 120
								AND MT.UserName <> ''  AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,sum(IRD.TransactionQty) ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,0 Total60Value
								,0 Total120Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 121 AND 365
								AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,sum(IRD.TransactionQty) Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,0 Total60Value
								,0 Total120Value
								,0 Total365Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 365 AND 900000
								AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName
							)x
							where  IsRegular='" + ValueOrNumber1 + @"'
							GROUP By MaterialType,CompanyId,CompanyName,MaterialTypeId";
				}
				else
				{
					sql = @"select CompanyId
							,CompanyName,PlantId,PlantName	
							,MaterialTypeId,MaterialType
							,ThirtyDaysCount= CASE WHEN CAST(sum(ThirtyDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(ThirtyDaysCount) AS INT) END
							,Total30Value=CASE WHEN CAST(Sum(Total30Value) AS INT) =0 THEN NULL ELSE CAST(Sum(Total30Value) AS INT) END
							,FourtyfiveDaysCount=CASE WHEN CAST(sum(FourtyfiveDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(FourtyfiveDaysCount) AS INT) END
							,Total45Value=CASE WHEN CAST(Sum(Total45Value) AS INT)=0 THEN NULL ELSE CAST(Sum(Total45Value) AS INT) END
							,SixtyDaysCount= CASE WHEN CAST(sum(SixtyDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(SixtyDaysCount) AS INT) END 
							,Total60Value=CASE WHEN CAST(sum(Total60Value) AS INT)=0 THEN NULL ELSE CAST(sum(Total60Value) AS INT) END
							,HundredtwentyDaysCount =CASE WHEN CAST(sum(HundredtwentyDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(HundredtwentyDaysCount) AS INT) END
							,Total120Value= CASE WHEN CAST(sum(Total120Value) AS INT) =0 THEN NULL ELSE CAST(sum(Total120Value) AS INT)  END
							,ThreeHundredSixtyfiveDaysCount=CASE WHEN CAST(sum(ThreeHundredSixtyfiveDaysCount) AS INT)=0 THEN NULL ELSE CAST(sum(ThreeHundredSixtyfiveDaysCount) AS INT) END
							,Total365Value=CASE WHEN CAST(sum(Total365Value) AS INT)=0 THEN NULL ELSE CAST(sum(Total365Value) AS INT) END
							,Transaction365QtyGrt=CASE WHEN CAST(sum(Transaction365QtyGrt) AS INT)=0 THEN NULL else CAST(sum(Transaction365QtyGrt) AS INT) END 
							--,Total366Value=CASE WHEN CAST(sum(Total366Value) AS INT) =0 THEN NULL ELSE CAST(sum(Total366Value) AS INT) END 
							,Total366Value=CASE WHEN CAST(sum(Total366Value) AS bigint) =0 THEN NULL ELSE CAST(sum(Total366Value) AS bigint) END 

							FROM
							(
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName,P.Id PlantId,P.UserName PlantName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,sum(IRD.TransactionQty) ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total30Value
								,0 Total45Value
								,0 Total60Value
								,0 Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 0 AND 30
								AND MT.UserName <> ''      AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName,P.Id ,P.UserName 	
							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	,P.Id PlantId,P.UserName PlantName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,sum(IRD.TransactionQty) FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total45Value
								,0 Total60Value
								,0 Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 31 AND 45
								AND MT.UserName <> ''      AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName,P.Id ,P.UserName 	

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	,P.Id PlantId,P.UserName PlantName		
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,sum(IRD.TransactionQty) SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total60Value
								,0 Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 46 AND 60
								AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName,P.Id ,P.UserName 	

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	,P.Id PlantId,P.UserName PlantName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,sum(IRD.TransactionQty) HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,0 Total60Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total120Value
								,0 Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 61 AND 120
								AND MT.UserName <> ''  AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName,P.Id ,P.UserName 	

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	,P.Id PlantId,P.UserName PlantName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,sum(IRD.TransactionQty) ThreeHundredSixtyfiveDaysCount
								,0 Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,0 Total60Value
								,0 Total120Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total365Value
								,0 Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 121 AND 365
								AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName,P.Id ,P.UserName 	

							UNION ALL
							SELECT			               
								IM.CompanyId,CMP.UserName CompanyName	,P.Id PlantId,P.UserName PlantName	
								,mm.IsRegular IsRegular	
								,MT.Id MaterialTypeId,isnull(MT.UserName, '') MaterialType
								,0 ThirtyDaysCount
								,0 FourtyfiveDaysCount
								,0 SixtyDaysCount
								,0 HundredtwentyDaysCount
								,0 ThreeHundredSixtyfiveDaysCount
								,sum(IRD.TransactionQty) Transaction365QtyGrt
								,0 Total30Value
								,0 Total45Value
								,0 Total60Value
								,0 Total120Value
								,0 Total365Value
								,sum(IRD.TotalMaterialBooksCurrencyAmount) Total366Value
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
										,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between 365 AND 900000
								AND MT.UserName <> '' AND MM.IsRegular='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							GROUP BY IM.CompanyId,isnull(MT.UserName, ''),mm.IsRegular,MT.Id,CMP.UserName,P.Id ,P.UserName 	
							)x
							where  " + paramters + " AND ISNULL(x.IsRegular,0)='" + ValueOrNumber1 + @"'
							GROUP By MaterialType,CompanyId,CompanyName,MaterialTypeId,PlantId,PlantName";
				}
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
				//return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			//CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//return Json(_inventoryDashboardService.MaterialAgeingStatusDashboard(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate, groupName, ValueOrNumber, queryString, queryStringProcess), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult InventoryList(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.ExpenseListLineChart(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult ExpenseListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.ExpenseListLineChart(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult RevenueListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.RevenueListLineChart(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult DymnamicExpenseList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string CompanyId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//return Json(_inventoryDashboardService.DymnamicExpenseList(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
			var json = Json(_inventoryDashboardService.DymnamicExpenseList(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
			json.MaxJsonLength = int.MaxValue;
			return json;

		}

		[HttpPost, Authorize]
		public ActionResult InventoryStatusDashboardPlant(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string CompanyId, string PlantId, string IsRegular)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.InventoryStatusDashboardPlant(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, CompanyId, PlantId, IsRegular), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult MaterialAgeingDashboardPlant(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string CompanyId, string PlantId, string IsRegular)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.MaterialAgeingDashboardPlant(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, CompanyId, PlantId, IsRegular), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult DymnamicExpenseListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_inventoryDashboardService.DymnamicExpenseListLineChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
		}
		[HttpPost, Authorize]
		public ActionResult ModalCompanyWiseDetails(string Category, string days, string companyId, string PlantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			companyId = identity.CompanyId;
			PlantId = identity.PlantId;
			return Json(_inventoryDashboardService.ModalBudgetWiseExpense(Category, days, companyId, PlantId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]		
		public JsonResult UpdateInActive(string ReqId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			_inventoryDashboardService.UpdateInActive(ReqId);
			return Json(new { Message = AplosMessage.Updated });
		}

		[HttpPost, Authorize]
		public JsonResult UpdateInActivePO(string POId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			_inventoryDashboardService.UpdateInActivePO(POId);
			return Json(new { Message = AplosMessage.Updated });
		}
		[HttpGet, Authorize]
		public JsonResult GetReqForPoDetail(string Id)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @" select PurchaseOrderDetail.InventoryReceiveId PoId
					   ,PurchaseOrderDetail.Id PoDetailId
					   ,Replace(CONVERT(VARCHAR(11), PurchaseOrderDetail.AddedDate, 106), ' ', '-') AddedDate
					   ,TransactionQty
					   ,TransactionRate
					   ,TransactionAmount 
					   ,PurchaseOrderDetail.GRNRcvQty
					   ,a.EmployeeName PreparedBy
					   ,b.EmployeeName CheckedBy 
					   ,c.EmployeeName ApprovedBy
					   ,map.GRNId
					   ,P.UserName PartyName
					   from trn.PurchaseOrderDetail 
					   left join trn.PurchaseOrder on PurchaseOrder.id=PurchaseOrderDetail.InventoryReceiveId
					   LEFT join [HKP].[Party] p ON p.Id=PurchaseOrder.PartyId
					   left join [SEC].[User] u on u.UserId=PurchaseOrder.AddedBy
					   LEFT JOIN EmployeeInformation a ON a.SystemId=u.EmployeeId
					   LEFT JOIN EmployeeInformation b ON b.SystemId=PurchaseOrder.CheckedBy
					   LEFT JOIN EmployeeInformation c ON c.SystemId=PurchaseOrder.AuthorizedBy
					   left join trn.POGGRNMap map on map.PoDetailId=PurchaseOrderDetail.id

                        where RequisitionDetailId='" + Id + "'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetPOForGRNDetail(string Id)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @" select PurchaseOrderDetail.InventoryReceiveId PoId
					   ,PurchaseOrderDetail.Id PoDetailId
					   ,Replace(CONVERT(VARCHAR(11), InventoryReceive.GRNDate, 106), ' ', '-') AddedDate
					   ,d.TransactionQty
					   ,d.MaterialTranRate TransactionRate
					   ,d.MaterialTranAmount TransactionAmount 
					 --  ,PurchaseOrderDetail.GRNRcvQty
					   ,a.EmployeeName PreparedBy
					   ,b.EmployeeName CheckedBy 
					   ,c.EmployeeName ApprovedBy
					   ,map.GRNId
					   ,P.UserName PartyName
					   from trn.POGGRNMap Map
					   left join trn.PurchaseOrderDetail ON PurchaseOrderDetail.id=Map.PoDetailId
					   left join trn.InventoryReceive on InventoryReceive.id=map.GRNId
					   left join (select InventoryReceiveId,sum(TransactionQty) TransactionQty,sum(MaterialTranRate) MaterialTranRate, sum(MaterialTranAmount) MaterialTranAmount from trn.InventoryReceivedetail group by InventoryReceiveId)d on map.GRNId=d.InventoryReceiveId

					   left join trn.PurchaseOrder on PurchaseOrder.id=PurchaseOrderDetail.InventoryReceiveId
					   LEFT join [HKP].[Party] p ON p.Id=InventoryReceive.PartyId
					   left join [SEC].[User] u on u.UserId=InventoryReceive.AddedBy
					   LEFT JOIN EmployeeInformation a ON a.SystemId=u.EmployeeId
					   LEFT JOIN EmployeeInformation b ON b.SystemId=InventoryReceive.CheckedBy
					   LEFT JOIN EmployeeInformation c ON c.SystemId=InventoryReceive.AuthorizedBy
					   where Map.PoDetailId='" + Id + "'";//204-27

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public JsonResult MaterialAgeingMGDataByType(string Id, string days, string companyId, string PlantId, bool ValueOrNumber, string queryString, string queryStringProcess, bool IsAsset)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string fromDate = "";
			string toDate = "";
			string RequestDay = "";
			if (days == "30") { fromDate = "0"; toDate = "30"; RequestDay = "30"; }
			if (days == "45") { fromDate = "31"; toDate = "45"; RequestDay = "45"; }
			if (days == "60") { fromDate = "46"; toDate = "60"; RequestDay = "60"; }
			if (days == "120") { fromDate = "61"; toDate = "120"; RequestDay = "120"; }
			if (days == "365") { fromDate = "121"; toDate = "365"; RequestDay = "365"; }
			if (days == "9000000") { fromDate = "366"; toDate = "900000"; RequestDay = "9000000"; }


			Session["fromDateNew"] = fromDate;
			Session["toDateNew"] = toDate;

			var sql = "";
			try
			{

				var ValueOrNumber1 = 0;
				if (ValueOrNumber == false)
				{
					ValueOrNumber1 = 0;
				}
				else
				{
					ValueOrNumber1 = 1;
				}
				var IsAsset1 = 0;
				if (IsAsset == false)
				{
					IsAsset1 = 0;
				}
				else
				{
					IsAsset1 = 1;
				}

				sql = @"SELECT   CMPGR.Id CompanyGroupid,CMPGR.UserName CompanyGroup
									--,CMP.Id CompanyId,CMP.UserName CompanyName
									,p.Id PlantId,p.UserName PlantName
                                    ,CMP.UserName CompanyName
									,MT.Id MaterialTypeId
									,isnull(MT.UserName, '') MaterialType
									,MGM.Id MaterialGroupId
									,isnull(MGM.UserName,'') MaterialGroup	
									,isnull(MG1.UserName,'') MaterialGroup1Name
									,isnull(MG2.UserName,'') MaterialGroup2Name
									,isnull(MG3.UserName,'') MaterialGroup3Name	
									,sum(IRD.TransactionQty) TotalQty	
									,sum(IRD.TotalMaterialBooksCurrencyAmount) TotalValue			                
								FROM TRN.InventoryMaterial AS IM
								LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
								LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
								LEFT JOIN org.Plant P On P.Id=IM.PlantId
								LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
								LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
								LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
								LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
								LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
								LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
								LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
								LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
								LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty
											,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
										   FROM [TRN].[InventoryReceiveDetail] GROUP BY InventoryReceiveId,InventoryMaterialId
										   ) IRD ON IRD.InventoryMaterialId=IM.Id
								LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id
		            
								LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
								WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between '" + fromDate + @"' And '" + toDate + @"'
									AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
									AND MaterialTypeId='" + Id + @"'	
									AND MaterialTypeId is not null
									--AND CMP.Id='C20171' AND P.id='20171'
								GROUP BY MT.UserName,MT.Id,MGM.Id ,MGM.UserName,MG1.UserName,MG2.UserName ,MG3.UserName, p.Id ,p.UserName, CMPGR.Id ,CMP.UserName
									,CMPGR.UserName--,CMP.Id ,CMP.UserName
									";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}

		}

		[HttpPost, Authorize]
		public JsonResult MaterialAgeingMaterialataByMG(string Id, string days, string companyId, string PlantId, bool ValueOrNumber, string queryString, string queryStringProcess, bool IsAsset)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string fromDate = "";
			string toDate = "";
			string RequestDay = "";
			if (days == "30") { fromDate = "0"; toDate = "30"; RequestDay = "30"; }
			if (days == "45") { fromDate = "31"; toDate = "45"; RequestDay = "45"; }
			if (days == "60") { fromDate = "46"; toDate = "60"; RequestDay = "60"; }
			if (days == "120") { fromDate = "61"; toDate = "120"; RequestDay = "120"; }
			if (days == "365") { fromDate = "121"; toDate = "365"; RequestDay = "365"; }
			if (days == "9000000") { fromDate = "366"; toDate = "900000"; RequestDay = "9000000"; }

			var sql = "";
			try
			{

				var ValueOrNumber1 = 0;
				if (ValueOrNumber == false)
				{
					ValueOrNumber1 = 0;
				}
				else
				{
					ValueOrNumber1 = 1;
				}
				var IsAsset1 = 0;
				if (IsAsset == false)
				{
					IsAsset1 = 0;
				}
				else
				{
					IsAsset1 = 1;
				}

				sql = @"SELECT  
							 CMP.Id CompanyId,CMP.UserName CompanyName
								,p.Id PlantId,p.UserName PlantName	
							,MT.Id MaterialTypeId
                            ,CMP.UserName CompanyName
							,isnull(MT.UserName, '') MaterialType
							,MGM.Id MaterialGroupId
							,isnull(MGM.UserName,'') MaterialGroup	
							,isnull(MG1.UserName,'') MaterialGroup1Name
							,isnull(MG2.UserName,'') MaterialGroup2Name
							,isnull(MG3.UserName,'') MaterialGroup3Name	
							,MM.Id,MM.UserName MaterialName
							,sum(IRD.TransactionQty) TotalQty	
							,sum(IRD.TotalMaterialBooksCurrencyAmount) TotalValue			
			                
						FROM TRN.InventoryMaterial AS IM
						LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
						LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
						LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
						LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
						LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
						LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
						LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
						LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount from [TRN].[InventoryReceiveDetail] group by InventoryReceiveId,InventoryMaterialId) IRD ON IRD.InventoryMaterialId=IM.Id
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id	            
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
						WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between '" + Session["fromDateNew"] + @"' And '" + Session["toDateNew"] + @"'
							AND MT.UserName <> '' AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"' AND MM.IsAsset='" + IsAsset1 + @"'
							--AND MT.Id='MAT-201712'	
							AND MaterialTypeId is not null
							ANd MGM.Id='" + Id + @"'
							--ANd MGM.Id='2019275'							
							--AND CMPGR.Id='CG20171' AND CMP.Id='C20171' AND P.id='20171'
						GROUP BY MT.UserName,MT.Id,MGM.Id ,MGM.UserName,MG1.UserName	,MG2.UserName ,MG3.UserName ,MM.Id ,mm.UserName
						, p.Id ,p.UserName, CMP.Id ,CMP.UserName, CMPGR.Id ,CMPGR.UserName,CMP.UserName ";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}

		}

		[HttpPost, Authorize]
		public JsonResult MaterialAgeingArticleDataByMaterial(string Id, string days, string companyId, string PlantId, bool ValueOrNumber, string queryString, string queryStringProcess, bool IsAsset)
		{


			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string fromDate = "";
			string toDate = "";
			string RequestDay = "";
			if (days == "30") { fromDate = "0"; toDate = "30"; RequestDay = "30"; }
			if (days == "45") { fromDate = "31"; toDate = "45"; RequestDay = "45"; }
			if (days == "60") { fromDate = "46"; toDate = "60"; RequestDay = "60"; }
			if (days == "120") { fromDate = "61"; toDate = "120"; RequestDay = "120"; }
			if (days == "365") { fromDate = "121"; toDate = "365"; RequestDay = "365"; }
			if (days == "9000000") { fromDate = "366"; toDate = "900000"; RequestDay = "9000000"; }

			var sql = "";
			try
			{

				var ValueOrNumber1 = 0;
				if (ValueOrNumber == false)
				{
					ValueOrNumber1 = 0;
				}
				else
				{
					ValueOrNumber1 = 1;
				}
				var IsAsset1 = 0;
				if (IsAsset == false)
				{
					IsAsset1 = 0;
				}
				else
				{
					IsAsset1 = 1;
				}

				sql = @"SELECT   CMPGR.Id CompanyGroupid,CMPGR.UserName CompanyGroup
								,CMP.Id CompanyId,CMP.UserName CompanyName,CMP.UserName CompanyName
								,p.Id PlantId,p.UserName PlantName	
								,MT.Id MaterialTypeId
								,isnull(MT.UserName, '') MaterialType
								,MGM.Id MaterialGroupId
								,isnull(MGM.UserName,'') MaterialGroup	
								,isnull(MG1.UserName,'') MaterialGroup1Name
								,isnull(MG2.UserName,'') MaterialGroup2Name
								,isnull(MG3.UserName,'') MaterialGroup3Name	
								,MM.Id MaterialMasterId,MM.UserName MaterialName
								,ART.Id Articeid,ART.StandardName ArticleName
								,sum(IRD.TransactionQty) TotalQty	
								,sum(IRD.TotalMaterialBooksCurrencyAmount) TotalValue		
			                
							FROM TRN.InventoryMaterial AS IM
							LEFT JOIN ORG.CompanyGroup CMPGR ON CMPGR.Id = IM.CompanyGroupId
							LEFT JOIN org.company CMP ON CMP.Id = IM.CompanyId
							LEFT JOIN org.Plant P On P.Id=IM.PlantId
							LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
							LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
							LEFT JOIN [HKP].[MaterialGroup1] MG1 ON MG1.Id=MGM.MaterialGroup1Id
							LEFT JOIN [HKP].[MaterialGroup2] MG2 ON MG2.Id=MGM.MaterialGroup2Id
							LEFT JOIN [HKP].[MaterialGroup3] MG3 ON MG3.Id=MGM.MaterialGroup3Id
							LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
							LEFT JOIN [HKP].[MaterialType] AS MT ON MGM.MaterialTypeId = MT.Id
							LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
							LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
							LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
							LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
							LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
							LEFt JOIN (Select InventoryReceiveId,InventoryMaterialId,Sum(TransactionQty) TransactionQty,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount from [TRN].[InventoryReceiveDetail] group by InventoryReceiveId,InventoryMaterialId) IRD ON IRD.InventoryMaterialId=IM.Id
							LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId = IR.Id		            
							LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId = TUoM.Id
							WHERE DATEDIFF(day, IR.GRNDate, getdate()) Between '" + Session["fromDateNew"] + @"' And '" + Session["toDateNew"] + @"'		                
								--AND MT.Id='MAT-201712'	
								 AND MaterialTypeId is not null
								--ANd MGM.Id='2019164'
								--ANd MGM.Id='2019275'
								AND MM.Id='" + Id + @"'
								AND MT.UserName <> '' 
								AND ISNULL(MM.IsRegular,0)='" + ValueOrNumber1 + @"'  AND MM.IsAsset='" + IsAsset1 + @"'
								--AND CMPGR.Id='CG20171' AND CMP.Id='C20171' AND P.id='20171'
							GROUP BY 	MT.UserName,MT.Id,MGM.Id ,MGM.UserName,MG1.UserName	,MG2.UserName 
							,MG3.UserName ,MM.Id ,mm.UserName ,ART.Id ,ART.StandardName, p.Id ,p.UserName, CMP.Id ,CMP.UserName, CMPGR.Id ,CMPGR.UserName,CMP.UserName ";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}

		}

		#region Inventory Dashboard Status

		[HttpPost, Authorize]
		public ActionResult InventoryDashboardStatusFun(string companyGroupId, string companyId, string PlantId, string factDate, string fromDate, string toDate, string groupName, bool ValueOrNumber, string queryString, string queryStringProcess)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			PlantId = identity.PlantId;

			return Json(_inventoryDashboardService.InventoryDashboardStatus(identity.CompanyGroupId, identity.CompanyId, PlantId, factDate, fromDate, toDate, groupName, ValueOrNumber, queryString, queryStringProcess), JsonRequestBehavior.AllowGet);
		}


		[HttpPost, Authorize]
		public ActionResult MaterialTypeWiseMaterial(string companyGroupId, string companyId, string PlantId, string factDate, string fromDate, string toDate, string groupName, bool ValueOrNumber, string queryString, string queryStringProcess,string MaterialTypeID)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			companyId = identity.CompanyId;
			PlantId = identity.PlantId;
			return Json(_inventoryDashboardService.MaterialTypeWiseMaterialStatus(identity.CompanyGroupId, identity.CompanyId, PlantId, factDate, fromDate, toDate, groupName, ValueOrNumber, queryString, queryStringProcess, MaterialTypeID), JsonRequestBehavior.AllowGet);
		}


		[HttpPost, Authorize]
		public ActionResult MaterialGroupWiseMaterial(string companyGroupId, string companyId, string PlantId, string factDate, string fromDate, string toDate, string groupName, bool ValueOrNumber, string queryString, string queryStringProcess, string MaterialGroupID)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			companyId = identity.CompanyId;
			PlantId = identity.PlantId;
			return Json(_inventoryDashboardService.MaterialGroupWiseMaterialStatus(identity.CompanyGroupId, identity.CompanyId, PlantId, factDate, fromDate, toDate, groupName, ValueOrNumber, queryString, queryStringProcess, MaterialGroupID), JsonRequestBehavior.AllowGet);
		}

        [HttpPost, Authorize]
        public ActionResult MaterialWiseArticle(string companyGroupId, string companyId, string PlantId, string factDate, string fromDate, string toDate, string groupName, bool ValueOrNumber, string queryString, string queryStringProcess, string MaterialID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            companyId = identity.CompanyId;
            PlantId = identity.PlantId;
            return Json(_inventoryDashboardService.MaterialWiseArticleStatus(identity.CompanyGroupId, identity.CompanyId, PlantId, factDate, fromDate, toDate, groupName, ValueOrNumber, queryString, queryStringProcess, MaterialID), JsonRequestBehavior.AllowGet);
        }
		#endregion

		[HttpGet, Authorize]
		public ActionResult RequisitionDetailsReport(string[] RequisitionDetailsRow)
		{
			try
			{
				
				string[] empIdList = null;
				foreach (string id in RequisitionDetailsRow)
				{
					empIdList = id.Split(',');

				}

				string requisitionDetailsRow = "";

				foreach (var item in empIdList)
				{
					if (string.IsNullOrEmpty(requisitionDetailsRow))
					{
						requisitionDetailsRow += "'','" + item+"'";
					}
					else
					{
						requisitionDetailsRow += ",'" + item + "'";
					}
				}


				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				ExcelEngine excelEngine = new ExcelEngine();
				IWorkbook workbook = _inventoryDashboardService.GetRequisitionDetailsReport(excelEngine, requisitionDetailsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

				string strFileName = "RequisitionDetailsReports.xlsx";
				workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
				workbook.Close();
			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);
			}

			return null;
		}

		[HttpGet, Authorize]
		public ActionResult POPurchaseDetailsReport(string[] POPurchaseDetailsId)
		{

			try
			{
				string poPurchaseDetailsId = "";
				foreach (var item in POPurchaseDetailsId)
				{
					if (string.IsNullOrEmpty(poPurchaseDetailsId))
					{
						poPurchaseDetailsId += "''," + item;
					}
					else
					{
						poPurchaseDetailsId += "," + item;
					}
				}
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				//AccountsStatusDashboardService accountsStatusDashboardService = new AccountsStatusDashboardService(_sqlRepository, _companyParallelCurrencyService);
				//return Json(_inventoryDashboardService.GetCompanyGroupInformation(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);

				ExcelEngine excelEngine = new ExcelEngine();
				IWorkbook workbook = _inventoryDashboardService.GetPOPurchaseDetailsReport(excelEngine, poPurchaseDetailsId, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

				string strFileName = "POPurchaseDetailsReport.xlsx";
				workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
				workbook.Close();
			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);
			}
			return null;
		}
	}
}