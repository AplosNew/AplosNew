function PayrollsConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/payroll-group', {
            templateUrl: 'payrolls/payrollgroup/aplos',
            controller: 'payrollGroupController'
        })
        .when('/salary-advance', {
            templateUrl: 'payrolls/loanadvancemaster/aplos',
            controller: 'loanAdvanceMasterController'
        })
        .when('/company-tax-contribution', {
            templateUrl: 'payrolls/companytaxcontribution/aplos',
            controller: 'companyTaxContributionController'
        })
        .when('/salary-advance-approval', {
            templateUrl: 'payrolls/salaryadvanceapproval/aplos',
            controller: 'salaryAdvanceApprovalController'
        })
        .when('/salary-advance-openning-balance', {
            templateUrl: 'payrolls/salaryAdvanceOpeningBalance/aplos',
            controller: 'salaryAdvanceOpeningBalanceController'
        })
        .when('/payroll-group-master', {
            templateUrl: 'payrolls/PayrollGroupMaster/aplos',
            controller: 'payrollGroupMasterController'
        })
        .when('/paidhours-employee-assign', {
            templateUrl: 'payrolls/PaidHoursEmployeeAssign/aplos',
            controller: 'paidHoursEmployeeAssignController'
        })
        .when('/salary-process-delete', {
            templateUrl: 'payrolls/SalaryProcessDelete/aplos',
            controller: 'salaryProcessDeleteController'
        })
        .when('/payrolls-payslips', {
            templateUrl: 'payrolls/ParollsReport/aplos',
            controller: 'ParollsReportController'
        })
        .when('/promotion-increment-approval', {
            templateUrl: 'payrolls/PromotionIncrementApproval/aplos',
            controller: 'PromotionIncrementApprovalController'
        })
        .when('/salary-structure-approval', {
            templateUrl: 'payrolls/SalaryStructureApproval/aplos',
            controller: 'SalaryStructureApprovalController'
        })
        .when('/salary-structure-unapproval', {
            templateUrl: 'payrolls/SalaryStructureUnApproval/SalaryStructureUnApproval',
            controller: 'SalaryStructureUnApprovalController'
        })
        .when('/leave-encashment-entry-tobedeleted', {
            templateUrl: 'payrolls/leaveEncashmentEntry/aplos',
            controller: 'LeaveEncashmentEntryController'
        })
        .when('/multiple-leave-encashment-tobedeleted', {
            templateUrl: 'payrolls/leaveEncashmentEntry/MultipleLeaveEncashment',
            controller: 'MultipleLeaveEncashmentController'
        })
        .when('/with-in-year-leave-encashment-tobedeleted', {
            templateUrl: 'payrolls/leaveEncashmentEntry/WithinYearLeaveEncashment',
            controller: 'WithinYearLeaveEncashmentController'
        })
        .when('/specific-date-leave-encashment-tobedeleted', {
            templateUrl: 'payrolls/leaveEncashmentEntry/SpecificDateLeaveEncashment',
            controller: 'SpecificDateLeaveEncashmentController'
        })
        .when('/salary-head', {
            templateUrl: 'payrolls/salaryhead/',
            controller: 'salaryHeadController'
        })
        .when('/salary-rule', {
            templateUrl: 'payrolls/salaryrule/',
            controller: 'salaryRuleController'
        })
        .when('/salary-payment-statements', {
            templateUrl: 'payrolls/SalaryPaymentStatements/',
            controller: 'salaryPaymentStatementsController'
        })
        .when('/payment-statements-BankCSV', {
            templateUrl: 'payrolls/SalaryPaymentStatements/BankStatementCSV',
            controller: 'salaryPaymentStatementsBankCSVController'
        })
        .when('/esic-statements', {
            templateUrl: 'payrolls/ESICStatements/Aplos',
            controller: 'esicStatementsController'
        })
        .when('/providentfund-statements', {
            templateUrl: 'payrolls/ProvidentFundStatementReportandCSV/Aplos',
            controller: 'providentFundStatementReportandCSVController'
        })
        //.when('/bonus-register', {
        //    templateUrl: 'payrolls/bonusregister/Aplos',
        //    controller: 'bonusRegisterController'
        //})
        .when('/welfare-return', {
            templateUrl: 'payrolls/welfarereturn/',
            controller: 'welfareReturnController'
        })
        .when('/gratuity-report', {
            templateUrl: 'payrolls/GratuityReport/',
            controller: 'gratuityReportController'
        })
        .when('/final-settlement', {
            templateUrl: 'payrolls/FinalSettlementVoucher/Aplos',
            controller: 'finalSettlementVoucherController'
        })
        
        .when('/salary-process-other-status', {
            templateUrl: 'payrolls/SalaryProcessOtherStatus/Aplos',
            controller: 'SalaryProcessOtherStatusController'
        })
        .when('/salary-process-other-status-new', {
            templateUrl: 'payrolls/SalaryProcessOtherStatusNew/Aplos',
            controller: 'SalaryProcessOtherStatusNewController'
        })
        .when('/salary-process', {
            templateUrl: 'payrolls/SalaryProcess/Aplos',
            controller: 'SalaryProcessController'
        })
        .when('/salary-process-new', {
            templateUrl: 'payrolls/SalaryProcessNew/Aplos',
            controller: 'SalaryProcessNewController'
        })
        .when('/daily-allowance', {
            templateUrl: 'payrolls/DailyAllowance/Aplos',
            controller: 'DailyAllowanceController'
        })
        .when('/daily-allowance-Confirmation', {
            templateUrl: 'payrolls/DailyAllowance/DailyAllowanceConfirmation',
            controller: 'DailyAllowanceConfirmationController'
        })
        .when('/daily-allowance-rate', {
            templateUrl: 'payrolls/DailyAllowance/DailyAllowanceRateEmpWise',
            controller: 'DailyAllowanceRateEmpWiseController'
        })
        .when('/daily-allowance-setting', {
            templateUrl: 'payrolls/DailyAllowanceSetting/Aplos',
            controller: 'DailyAllowanceSettingController'
        })
        .when('/final-settlement-entry', {
            templateUrl: 'payrolls/FinalSettlement/Aplos',
            controller: 'finalSettlementController'
        })
        .when('/final-settlement-entry-new', {
            templateUrl: 'payrolls/FinalSettlement/Aplos',
            controller: 'finalSettlementNewController'
        })
        .when('/pay-slip', {
            templateUrl: 'payrolls/payslips/aplos',
            controller: 'paySlipsController'
        })
        .when('/pay-slip-new', {
            templateUrl: 'payrolls/payslipsnew/PaySlipsNew',
            controller: 'paySlipsNewController'
        })
        .when('/attdn-slip', {
            templateUrl: 'payrolls/AttendanceSlip/aplos',
            controller: 'attendanceSlipController'
        })
        .when('/daily-allowance-transaction', {
            templateUrl: 'payrolls/dailyallowancetransaction/aplos',
            controller: 'dailyAllowanceTransactionController'
        })
        .when('/bulk-increment', {
            templateUrl: 'payrolls/BulkIncrement/aplos',
            controller: 'BulkIncrementController'
        })
        .when('/bulk-increment-upload', {
            templateUrl: 'payrolls/BulkIncrementSalaryStructureDataUpload/aplos',
            controller: 'BulkIncrementSalaryStructureDataUploadController'
        })
        .when('/maternity-benefit', {
            templateUrl: 'payrolls/MaternityBenefit/aplos',
            controller: 'maternityBenefitController'
        })
        .when('/maternity-benefit-after', {
            templateUrl: 'payrolls/MaternityBenefitAfter/aplos',
            controller: 'maternityBenefitAfterController'
        })

        .when('/salary-certificate-report', {
            templateUrl: 'payrolls/SalaryPaymentStatements/SalaryCertificate',
            controller: 'salaryCertificateReportController'
        })
        .when('/advance-and-tds', {
            templateUrl: 'payrolls/AdvanceAndTDS/Aplos',
            controller: 'advanceAndTDSController'
        })
        .when('/esic-summary-report', {
            templateUrl: 'payrolls/ESICSummary/Aplos',
            controller: 'esicSummaryController'
        })

        .when('/increment-type', {
            templateUrl: 'payrolls/IncrementType/Aplos',
            controller: 'incrementGroupController'
        })
        .when('/salary-head-wise-payment-mode-policy', {
            templateUrl: 'payrolls/SalaryHeadWisePaymentModePolicy/Aplos',
            controller: 'salaryHeadWisePaymentModePolicyController'
        })
        .when('/encashment-report', {
            templateUrl: 'payrolls/Encashment/Aplos',
            controller: 'EncashmentController'
        })
        .when('/earn-leave-report', {
            templateUrl: 'payrolls/Encashment/EarnLeaveReport',
            controller: 'EncashmentController'
        })
        .when('/salary-structure-upload', {
            templateUrl: 'payrolls/SalaryStructureDataUpload/Aplos',
            controller: 'SalaryStructureDataUploadController'
        })
        .when('/salary-head-wise-amount-transaction', {
            templateUrl: 'payrolls/SalaryHeadWiseAmountTransaction/Aplos',
            controller: 'SalaryHeadWiseAmountTransactionController'
        })
        .when('/employee-fixed-service-master', {
            templateUrl: 'payrolls/EmployeeFixedServicMaster/Aplos',
            controller: 'EmployeeFixedServicMasterController'
        })
        .when('/employee-fixed-service-transaction', {
            templateUrl: 'payrolls/EmployeeFixedServicTransaction/Aplos',
            controller: 'EmployeeFixedServicTransactionController'
        })
        .when('/bonus-retained-disbursement', {
            templateUrl: 'payrolls/BonusRetainedDisbursement/Aplos',
            controller: 'BonusRetainedDisbursementController'
        })

        .when('/final-settlement-deduction-head', {
            templateUrl: 'payrolls/FinalSettlementDeductionHead/Aplos',
            controller: 'FinalSettlementDeductionHeadController'
        })

        .when('/currency-rule', {
            templateUrl: 'payrolls/CurrencyRule/Aplos',
            controller: 'CurrencyRuleController'
        })

        .when('/bank-cash-setting', {
            templateUrl: 'Payrolls/BankCashPercentageSetting/Aplos',
            controller: 'BankCashPercentageSettingController'
        })
        .when('/external-data-upload', {
            templateUrl: 'Payrolls/ExternalDataUploadFromExcel/Aplos',
            controller: 'ExternalDataUploadFromExcelController'
        })

        .when('/employee-bank-info', {
            templateUrl: 'Payrolls/EmployeeBankAccountInfo/Aplos',
            controller: 'EmployeeBankAccountInfoController'
        })

        .when('/arrear', {
            templateUrl: 'Payrolls/Arrear/Aplos',
            controller: 'ArrearController'
        })
        .when('/arrear-approval', {
            templateUrl: 'Payrolls/ArrearApproval/Aplos',
            controller: 'ArrearApprovalController'
        })

        .when('/tax-policy', {
            templateUrl: 'Payrolls/TaxPolicy/Aplos',
            controller: 'TaxPolicyController'
        })

        .when('/bonus-process', {
            templateUrl: 'Payrolls/BonusProcess/Aplos',
            controller: 'BonusProcessController'
        })

        .when('/tax-type', {
            templateUrl: 'Payrolls/TaxType/Aplos',
            controller: 'TaxTypeController'
        })


        .when('/employee-service-variable', {
            templateUrl: 'Payrolls/EmployeeServiceVariable/Aplos',
            controller: 'EmployeeServiceVariableController'
        })

        .when('/professional-tax-ob', {
            templateUrl: 'Payrolls/ProfessionalTaxOB/Aplos',
            controller: 'ProfessionalTaxOBController'
        })

        .when('/tax-ob', {
            templateUrl: 'Payrolls/TaxOB/Aplos',
            controller: 'TaxOBController'
        })

        .when('/employee-income-tax', {
            templateUrl: 'Payrolls/EmployeeIncomeTax/Aplos',
            controller: 'EmployeeIncomeTaxController'
        })

        .when('/income-tax-process', {
            templateUrl: 'Payrolls/EmployeeIncomeTaxProcess/Aplos',
            controller: 'EmployeeIncomeTaxProcessController'
        })

        .when('/late-deduction', {
            templateUrl: 'Payrolls/LateDeduction/Aplos',
            controller: 'LateDeductionController'
        })

        .when('/employee-day-status-report', {
            templateUrl: 'Payrolls/EmployeeDayStatusReport/Aplos',
            controller: 'EmployeeDayStatusReportController'
        })
        .when('/increment-report', {
            templateUrl: 'Payrolls/IncrementReport/Aplos',
            controller: 'IncrementReportController'
        })
        .when('/increment-report-summary', {
            templateUrl: 'Payrolls/IncrementReportSummary',
            controller: 'IncrementReportSummaryController'
        })


        .when('/gratuity-insurance-agreement', {
            templateUrl: 'Payrolls/GratuityInsuranceAgreement/Aplos',
            controller: 'GratuityInsuranceAgreementController'
        })

        .when('/individual-gratuity-policy', {
            templateUrl: 'Payrolls/IndividualGratuityPolicy/Aplos',
            controller: 'IndividualGratuityPolicyController'
        })
        .when('/graruity-insurance-report', {
            templateUrl: 'Payrolls/GraruityInsuranceReport/Aplos',
            controller: 'GraruityInsuranceReportController'
        })
        .when('/notice-period-setting', {
            templateUrl: 'Payrolls/NoticePeriodSetting/Aplos',
            controller: 'NoticePeriodSettingController'
        })      
        .when('/tax-saving-item', {
            templateUrl: 'Payrolls/TaxSavingItem/Aplos',
            controller: 'TaxSavingItemController'
        })
        .when('/tax-saving-group', {
            templateUrl: 'Payrolls/TaxSavingGroup/Aplos',
            controller: 'TaxSavingGroupController'
        })
        .when('/pay-slip-cont', {
            templateUrl: 'Payrolls/PaySlips/PaySlipContractor',
            controller: 'paySlipsContractorController'
        })
        .when('/company-providentfund-statements', {
            templateUrl: 'Payrolls/CompanyProvidentFundStatementReport/Aplos',
            controller: 'CompanyProvidentFundStatementReportController'
        })
        .when('/company-esic-statements', {
            templateUrl: 'Payrolls/ESICStatementsCompany/Aplos',
            controller: 'ESICStatementsCompanyController'
        })
        .when('/company-gratuity-report', {
            templateUrl: 'Payrolls/GratuityReportCompany/Aplos',
            controller: 'GratuityReportCompanyController'
        })
        .when('/employee-advance-deduction', {
            templateUrl: 'Payrolls/EmployeeAdvanceDeduction/Aplos',
            controller: 'EmployeeAdvanceDeductionController'
        })
        .when('/company-wise-external-data-upload-from-excel', {
            templateUrl: 'Payrolls/CompanyWiseExternalDataUploadFromExcel/Aplos',
            controller: 'CompanyWiseExternalDataUploadFromExcelController'
        })
        .when('/company-wise-bank-sheet', {
            templateUrl: 'Payrolls/CompanyWiseBankSheet/Aplos',
            controller: 'CompanyWiseBankSheetController'
        })
        .when('/ot-formula', {
            templateUrl: 'Payrolls/OTFormula/Aplos',
            controller: 'OTFormulaController'
        })
        .when('/tax-policy-master', {
            templateUrl: 'Payrolls/TaxPolicyHeader/Aplos',
            controller: 'TaxPolicyHeaderController'
        })
        .when('/emp-sep-setup', {
            templateUrl: 'Payrolls/EmployeeSeperationSetup/Aplos',
            controller: 'EmployeeSeperationSetupController'
        })

        .when('/final-settlement', {
            templateUrl: 'payrolls/FinalSettlement/FinalSettle',
            controller: 'fullandfinalSettlementController'
        })
        .when('/fnfapprove', {
            templateUrl: 'payrolls/FinalSettlement/Approve',
            controller: 'fullandfinalSettlementApproveController'
        })
        .when('/fnfpayment', {
            templateUrl: 'payrolls/FinalSettlement/Payment',
            controller: 'fullandfinalSettlementPaymentController'
        })
        .when('/fnfreport', {
            templateUrl: 'payrolls/FinalSettlement/Report',
            controller: 'FNFReportController'
        })
        .when('/advice-report', {
            templateUrl: 'payrolls/PaySlipsNew/SalaryAdvice',
            controller: 'SalaryAdviceController'
        })
        .when('/salaryrulesetup', {
            templateUrl: 'payrolls/EmployeeSalaryRuleSetup/Aplos',
            controller: 'EmployeeSalaryRuleSetupController'
        })
        .when('/empsalarystructure', {
            templateUrl: 'payrolls/EmployeeSalaryRuleSetup/SalaryStructure',
            controller: 'EmployeeSalaryStructureController'
        })
        ;


}
PayrollsConfig.$inject = ['$routeProvider', '$locationProvider'];