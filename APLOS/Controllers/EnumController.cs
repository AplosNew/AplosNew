using Aplos.Service.Enums;
using Library.Model.Banks;
using Library.Model.Employees;
using Library.Model.Enums;
using Library.Model.ManagementChartOfAccounts;
using Library.Model.Parties;
using Library.Model.Payments;
using Library.Model.Setups;
using Library.Model.Taxations;
using Library.Service.ChartOfAccounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Systems;
using Library.Service.Taxations;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class EnumController : BaseController
    {
        [HttpGet, Authorize]
        public JsonResult GetCboConsumtionBooking()
        {
            return Json(EnumService.GetEnumCbo<ConsumtionBooking>(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboRoundingType()
        {
            return Json(EnumService.GetEnumCbo<RoundingType>(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getSpecifyToEnumCbo()
        {
            return Json(EnumService.GetEnumCbo<SpecifyTo>(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult getPackingTypeEnumCbo()
        {
            return Json(EnumService.GetEnumCbo<PackingType>(), JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet, Authorize]
        public JsonResult GetCboPaymentType()
        {
            return Json(EnumService.GetEnumCbo<PaymentType>(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboLabelNameInLocalLanguage()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<LabelNameInLocalLanguage>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboSalaryHeadNameInLocalLanguage()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<LabelNameInLocalLanguage>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboSourceType()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<SourceType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCustomerPartnerFunctionListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumVendorPartnerFunctionList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVendorPartnerFunctionListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumVendorPartnerFunctionList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPartyAccountGroupTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PartyType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAccountTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<AccountTypeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryJVTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<SalaryJVType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAccountGroupTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<AccountGroupGLTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBeneficiaryTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BeneficiaryType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetNewBeneficiaryTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<NewBeneficiaryType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBalanceSheetLevelCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BalanceSheetLevel>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetIncomeStatementCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<IncomeStatementLevel>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTrailBalanceLevelCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TrailBalanceLevel>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetFSDTrailBalanceLevelCbo()
        //{
        //    return Json(new SelectList(EnumService.GetEnumCbo<TrailBalanceLevel>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetCostCenterTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<CostCenterType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAccountGroupBalanceTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<AccountGroupBalanceTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPeriodListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PKGeneratorEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboAccountType()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BankACType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult TaxCodeTypeList()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxCodeTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboResponsiblePersonMappingLevel()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ResponsiblePersonMappingLevel>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboEntityType()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<Library.Model.Organizations.EntityType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMailServiceNameCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<MailServiceName>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPaymentLinkCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PaymentLink>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmploymentTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EmploymentType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEnumPartyTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PartyType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetUsageTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<UsageTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRequirementTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<RequirementTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetHazardsListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<HazardsList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFlammabilityListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FlammabilityList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPurchaseFrequencyListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PurchaseFrequencyList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboFALinked()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FALinked>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPaymentModeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PaymentSource>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ActivityType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBudgetForCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BudgetFor>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAssetAttributeForCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumAssetAttribute>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobWorktypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumProcessSetDetailJobWorkTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobDescriptionLevelListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumJobDescriptionLevelList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobDescriptionPrimaryOrSecondaryListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumJobDescriptionPrimaryOrSecondaryList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobDescriptionFrequencyListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumJobDescriptionFrequencyList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobDescriptionNatureOrActivityListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumJobDescriptionNatureOrActivityList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobDescriptionSystemOrManualListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumJobDescriptionSystemOrManualList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumRequiredTimeUnitCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumRequiredTimeUnit>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumProcessNatureCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumProcessNature>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEnumProductionBookingLevelCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ProductionBookingLevel>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumForProjectPlanningStatus()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumStatusForProjectPlanning>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumForDocumentType()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumDocumentType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEnumEnumPlanningTypes()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumPlanningTypes>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEnumForImportance()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumImportance>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumEmploymentStage()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumEmploymentStage>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumPostRecruitment()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumPostRecruitment>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumDependateDate()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumDependateDate>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSelectionStatus()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<SelectionStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetConfirmationStatus()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ConfirmationStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBusinessProcess()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BusinessProcessEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAttributePropertiesCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<AttributePropertiesEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetValueAssignmentCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ValueAssignmentEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetApprovalStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumResignationApprovalStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryApprovalStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumSalaryApprovalStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSupervisorActionStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<SupervisorActionStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAuthorizationCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<Authorization>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLetterTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<LetterType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDocumentationByCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumDocumantationBy>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProfileTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ProfileType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetComplianceDocumentCategoryEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<RelatedType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDurationUOMEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<DurationUOM>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumOrderGradeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumOrderGrade>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumOrderStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumOrderStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOrderStatusEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<OrderStatusEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumManpowerTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumManpowerTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumStyleCategoryListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumStyleCategory>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDataSourceCategoryEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<DataSourceCategory>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDocumentFormateEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<DocumentFormate>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityCategoryEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ActivityCategory>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPeriodEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<Period>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityImportanceEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ActivityImportance>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDevelopmentCategoryEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<DevelopmentCategory>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetProductionProcessGroupEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumProductionProcessGroupJobWorkTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPartyGLTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PartyGLType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxCircleEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxCircle>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxForCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxFor>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDifferentInCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxDifferentIn>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxApplicableCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxApplicable>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBuyerActivityCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BuyerActivity>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetInquiryActivityCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<InquiryActivity>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLeaveApplyCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<AppliedBy>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobWorkTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumProcessSetDetailJobWorkTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEnumJobWorkTypeListCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<EnumJobWorkTypeList>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetExpensesBookingApprovalStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ApprovalStatus>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPOApprovalStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<Approval>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCheckedStatusCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<Checked>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetExpensesActivityTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ExpensesActivityType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFuleTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FuelType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetTransportTypeCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TransportType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalaryHeadEnum()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<SalaryHeadEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSalaryPayableGroupEnum()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<SalaryPayableGroup>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPaymentModeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PaymentModeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEMPPaymentModeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<Library.Service.Enums.PaymentMode>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetChargesTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ChargesType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCostingTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<CostingType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostingSegmentEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<CostingSegment>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAcceptancePaymentSourceEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<AcceptancePaymentSource>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFundUtilizationEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FundUtilization>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBuyerDeductionEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<BuyerDeduction>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPaymentBasedOnEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<PaymentBasedOn>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetDailyAllowanceCatagoryEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<DailyAllowanceCatagoryEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetNotificationEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<NotificationEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetGeneralAccountDeterminateEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<GeneralAccountDeterminateEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFixedAssetRegisterStatusEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FixedAssetRegisterStatusEmum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<MaterialTypeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetFarmingBusinessProcessenumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FarmingBusinessProcess>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFixedAssetDisposeStatusEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<FixedAssetDisposeStatusEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobWorkTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<JobWorkType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetTaxCategoryTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxCategoryTypeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxCategoryCodeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxCategoryCodeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTaxCategoryLevelEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TaxCategoryLevelEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetActivityOrderTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ActivityOrderTypeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetValueOfDistributionEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<ValueOfDistributionEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetWorkingDaysInAMonthEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<WorkingDaysInAMonth>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetOrderTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<OrderType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetTermsAndConditionsEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TermsAndConditionsEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetTableColumnEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<TableColumnEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDataTypeEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<DataType>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPOInvoiceCriteriaEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<POInvoiceCriticality>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCostingSOEnumCbo()
        {
            return Json(new SelectList(EnumService.GetEnumCbo<CostingSO>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMobileAppMenuEnumCbo()
        {
            return Json(new SelectList(items: EnumService.GetEnumCbo<MobileAppMenuEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingTypesEnumCbo()
        {
            return Json(new SelectList(items: EnumService.GetEnumCbo<PackingTypeEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBarcodeGeneratorSettingEnumCbo()
        {
            return Json(new SelectList(items: EnumService.GetEnumCbo<BarcodeGeneratorSettingEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmployeeSeprationSetupEnumCbo()
        {
            return Json(new SelectList(items: EnumService.GetEnumCbo<EmployeeSeprationSetupEnum>(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

      
    }
}