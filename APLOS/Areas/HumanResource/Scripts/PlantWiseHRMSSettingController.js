'use strict';
PlantWiseHRMSSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function PlantWiseHRMSSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Plant Wise HRMS Setting';
    $scope.Path = 'HumanResource/PlantWiseHRMSSetting/';
    $scope.plantList = [];
    $scope.Action = 'Save';

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.plantList = [];
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        $http({
            method: 'POST',
            url: $scope.Path + "GetModPlant",
            data: { CompanyId: $scope.PlantWiseHRMSSetting.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
    }
    $scope.PlantWiseHRMSSettingMain = {
        SystemID: null,
        GroupID: null,
        PlantID: null,
        PlantPrefix: null,
        AttdnProcBase: null,
        IsOTOverHalfDay: 'true',
        IsOTBasedOnPerMinute: true,
        MinimumOTMinute: 20,
        IsRoundOptionApplicable: false,
        RoundFigureForOT: 15,
        OTConsiderOn: 'Decimal Value',
        OperationSetting: null,
        IsPreallocationBasedOT: false,
        IsOTConfirmationAuto: false,
        IsOutMissingValidationRequired: false,
        PayableMinimumOT: 30,
        OTFractionCalculation: 'ROUND',
        DOCBaseON: 'Month',
        IsPastDOJAllowed: true,
        PastDOJDaysAllowed: 25,
        ProbationPeriodAlertBeforeDays: 7,
        ResignationAlertBeforeDays: 3,
        IsEmployeeCodeOpenField: true,
        DefaultWeekOff: null,
        IsCityMandatory: false,
        IsReferenceRequired: false,
        NoPunchOnLeave: false,
        DOCCount: 6,
        CallAttendanceAfterProfileEntry: false,
        IsOTConfirmationAfterLock: false,
        ProcessSalaryForSeparatedWithZeroPresent: false,
        ShiftBasedPunchFlag: false,
        NoPunchOnHoliday: false,
        IncrementAlertBeforeDays: 30,
        LongTermAbesnteeism: 10,
        IsLongAbsenteeismAuto: false,
        TBSDays: 20,
        IsTBSAuto: false,
        IsOTConfirmationAutoForZero: false,
        IsRemoteAttendanceApprovalRequired: true,
        IsSandwichAbsentInWeekend: true,
        IsSandwichAbsentInHoliday: true,
        IsPriorityOfHolidayOverWeekOff: false,
        NoPunchOnWeekoff: false,
        IsAutoEmpCodeWithPrefix: false,
        EmpCodeStartValue: 0,
        EmployeeCodeStart: null,
        EmployeeCodeCheckLevel: null,
        DateAdded: new Date(),
        ResultendOT: null,
        IsSalaryStructureShowInEIReport: false
        ,EmployeeOperationBackDateAllow:2
    }
    $scope.PlantWiseHRMSSetting = Object.assign({}, $scope.PlantWiseHRMSSettingMain);
    $scope.MasterList = [];
    $scope.getData = function () {
        $scope.MasterList = [];
        $scope.PlantID = $scope.PlantWiseHRMSSetting.PlantID;
        $scope.CompanyId = $scope.PlantWiseHRMSSetting.CompanyId;
        $scope.PlantWiseHRMSSetting = Object.assign({}, $scope.PlantWiseHRMSSettingMain);
        $scope.PlantWiseHRMSSetting.PlantID = $scope.PlantID;
        $scope.PlantWiseHRMSSetting.CompanyId = $scope.CompanyId;
        $http({
            method: 'POST',
            url: $scope.Path + "GetList",
            data: { CompanyId: $scope.PlantWiseHRMSSetting.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MasterList = response.data;
        });
    }

    $scope.Lock = function () {
        if ($scope.PlantWiseHRMSSetting.IsOTConfirmationAfterLock == true) {
            $scope.PlantWiseHRMSSetting.IsOTConfirmationAutoForZero = false;
        }
    }
    $scope.EmployeeCodeOpenField = function () {
        if ($scope.PlantWiseHRMSSetting.IsEmployeeCodeOpenField == true) {
            $scope.PlantWiseHRMSSetting.EmployeeCodeStart = null;
            $scope.PlantWiseHRMSSetting.EmpCodeStartValue = null;
            $scope.PlantWiseHRMSSetting.IsAutoEmpCodeWithPrefix = false;
        }
    }

    $scope.get = function (obj) {
        $scope.PlantWiseHRMSSetting = obj;
        $scope.PlantID = $scope.PlantWiseHRMSSetting.PlantID;
        $scope.CompanyId = $scope.PlantWiseHRMSSetting.CompanyId;
        if (baseService.isUndefinedOrNull($scope.PlantWiseHRMSSetting.SystemID)) {
            $scope.PlantWiseHRMSSetting = Object.assign({}, $scope.PlantWiseHRMSSettingMain);
            $scope.Action = 'Save';
        } else {
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay = 'false') {
                $scope.PlantWiseHRMSSetting.IsOTOverHalfDay = 'true';
            }
            else {
                $scope.PlantWiseHRMSSetting.IsOTOverHalfDay = 'false';
            }
            $scope.Action = 'Update';
        }
        $scope.PlantWiseHRMSSetting.PlantID = $scope.PlantID;
        $scope.PlantWiseHRMSSetting.CompanyId = $scope.CompanyId;
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.Save = function () {
        try {
            if ($scope.PlantWiseHRMSSetting.PlantID == null || $scope.PlantWiseHRMSSetting.PlantID == '' || $scope.PlantWiseHRMSSetting.PlantID == 'undefined') {
                throw "Plant Can't be Blank"
            }
            if ($scope.PlantWiseHRMSSetting.IsEmployeeCodeOpenField == true) {
                $scope.PlantWiseHRMSSetting.EmployeeCodeStart = '';
                $scope.PlantWiseHRMSSetting.EmpCodeStartValue = 0;
            }
            if ($scope.PlantWiseHRMSSetting.EmployeeCodeStart != 'AutoIncrement') {
                $scope.PlantWiseHRMSSetting.EmpCodeStartValue = 0;
                $scope.PlantWiseHRMSSetting.IsAutoEmpCodeWithPrefix = false;
            }
            if ($scope.PlantWiseHRMSSetting.IsRoundOptionApplicable == true) {
                $scope.PlantWiseHRMSSetting.RoundFigureForOT = 0;
                $scope.PlantWiseHRMSSetting.OTFractionCalculation = '';
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'false') {
                $scope.PlantWiseHRMSSetting.IsOTBasedOnPerMinute = false;
                $scope.PlantWiseHRMSSetting.MinimumOTMinute = 0;
                $scope.PlantWiseHRMSSetting.IsRoundOptionApplicable = false;
                $scope.PlantWiseHRMSSetting.RoundFigureForOT = 0;
                $scope.PlantWiseHRMSSetting.PayableMinimumOT = 0;
                $scope.PlantWiseHRMSSetting.OTFractionCalculation = null;
            }
            if ($scope.PlantWiseHRMSSetting.IsOTConfirmationAuto == true) {
                $scope.PlantWiseHRMSSetting.IsOTConfirmationAutoForZero = false;
                $scope.PlantWiseHRMSSetting.IsOTConfirmationAfterLock = false;
            }
            if ($scope.PlantWiseHRMSSetting.IsOTConfirmationAfterLock == true) {
                $scope.PlantWiseHRMSSetting.IsOTConfirmationAutoForZero = false;
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'true' && $scope.PlantWiseHRMSSetting.IsOTBasedOnPerMinute == true) {

                $scope.PlantWiseHRMSSetting.MinimumOTMinute = 0;
                $scope.PlantWiseHRMSSetting.IsRoundOptionApplicable = false;
                $scope.PlantWiseHRMSSetting.RoundFigureForOT = 0;
                $scope.PlantWiseHRMSSetting.PayableMinimumOT = 0;
                $scope.PlantWiseHRMSSetting.OTFractionCalculation = null;
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'true' && $scope.PlantWiseHRMSSetting.IsOTBasedOnPerMinute == false && $scope.PlantWiseHRMSSetting.IsRoundOptionApplicable == false) {

                $scope.PlantWiseHRMSSetting.RoundFigureForOT = 0;
                $scope.PlantWiseHRMSSetting.OTFractionCalculation = null;
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'true' && $scope.PlantWiseHRMSSetting.IsOTBasedOnPerMinute == false && $scope.PlantWiseHRMSSetting.MinimumOTMinute == 0) {
                throw "Considerable Minimum OT Limit is Required";
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'true' && $scope.PlantWiseHRMSSetting.IsOTBasedOnPerMinute == false && $scope.PlantWiseHRMSSetting.PayableMinimumOT == 0) {
                throw "Payable Minimum OT is Required";
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'true' && $scope.PlantWiseHRMSSetting.IsRoundOptionApplicable == true && $scope.PlantWiseHRMSSetting.RoundFigureForOT == 0) {
                throw "Round Figure For OT is Required";
            }
            if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay == 'true' && $scope.PlantWiseHRMSSetting.IsRoundOptionApplicable == true && $scope.PlantWiseHRMSSetting.OTFractionCalculation == null) {
                throw "OT Fraction Calculation is Required";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.PlantWiseHRMSSettingNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.Path + "Create",
                    data: { 'data': $scope.PlantWiseHRMSSetting },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getPlant();
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.getPlant = function () {
        $scope.PlantID = $scope.PlantWiseHRMSSetting.PlantID;
        $scope.CompanyId = $scope.PlantWiseHRMSSetting.CompanyId;
        $scope.PlantWiseHRMSSetting = Object.assign({}, $scope.PlantWiseHRMSSettingMain);
        $scope.PlantWiseHRMSSetting.PlantID = $scope.PlantID;
        $scope.PlantWiseHRMSSetting.CompanyId = $scope.CompanyId;
        $http({
            method: 'POST',
            url: $scope.Path + "GetPlantList",
            data: { PlantID: $scope.PlantWiseHRMSSetting.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PlantWiseHRMSSetting = response.data[0];
            if (baseService.isUndefinedOrNull($scope.PlantWiseHRMSSetting)) {
                $scope.PlantWiseHRMSSetting = Object.assign({}, $scope.PlantWiseHRMSSettingMain);
                $scope.Action = 'Save';
            }
            else {
                if ($scope.PlantWiseHRMSSetting.IsOTOverHalfDay = 'false') {
                    $scope.PlantWiseHRMSSetting.IsOTOverHalfDay = 'true';
                }
                else {
                    $scope.PlantWiseHRMSSetting.IsOTOverHalfDay = 'false';
                }
                $scope.Action = 'Update';
            }
            $scope.PlantWiseHRMSSetting.PlantID = $scope.PlantID;
            $scope.PlantWiseHRMSSetting.CompanyId = $scope.CompanyId;
            
        });
    }

}