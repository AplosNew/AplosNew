'use strict';
leavePolicyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function leavePolicyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Leave Policy New';
    $scope.Action = 'Save';
    $scope.ActionDetails = 'SaveDetails';
    $scope.index = -1;
    $scope.path = 'Leave/LeavePolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveDetailsUrl = $scope.path + 'SaveDetails';
    $scope.updateDetailsUrl = $scope.path + 'SaveDetails';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteDetailsUrl = $scope.path + 'DeleteDetails/';

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.LeavePolicyModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }


    $scope.LeavePolicyModel = {
        SystemID: null,
        PolicyCode: null,
        PolicyName: null,
        DefaultPolicy: false,
        PlantID: false,
        CompanyId: false,
    };

    $scope.LeavePolicyModelDetails = {
        SystemID: null,
        LPMSystemID: null,
        LTSystemID: null,
        IsWithoutPay: null,
        IsWithoutPay: 0,
        IsCarryForward: null,
        CarryForwardDay: 0,
        IsMaxAllocation: null,
        MaxAllocationLimit: 0,
        MinAllocationLimit: 0,
        CalculationBasis: null,
        LeaveCredit: null,
        IsExcessAllow: false,
        IsPrecedingWeekoff: false,
        IsSucceedignHoliday: false,
        IsSucceedignWeekoff: false,
        IsSucceedignHoliday: false,

        InBetweenWeekoff: false,
        InBetweenHoliday: false,
        IsAsperEntryOnW: true,
        IsAsperEntryOnH: true,
        IsNoLeaveOnW: false,
        IsNoLeaveOnH: false,

        LeaveInHourDaily: false,
        IsActive: false,
        GroupID: null,
        PlantID: null,
        LeaveCalculationRoundOption: null,
        IsMaxEncashmentLapse: false,
        MaxEncashment: 0,
        IsMaxEncashmentLapse: false,
        MaxEncashmentLapse: 0,
        IsAllowed: 'true',
        IsAllowedonspecialappeal: false,
        IsProratacurrentyear: false,
        IsNewlyJoined: false,
        NewlyJoined: 0,
        EncashWorkingDaysQty: 0,
        EncashEarnLeaveQty: 0,
        IsSubmittoApproval: false,
        IsAvailPreviousYearProRata: false,
        IsAvailCurrentYearProRata: false,
        IsAvailExceptionAllowedOnSpecialAppeal: false,
        AllowedAfterDays: 0,
        IsPostApplicationAllowed: false,
        IsExceptionAllowed: false,
        IsSubjectToApproval: false,
        IsProofDocRequired: false,
        ProofDocReqAfterDays: 0,
        LvCalculationOnDOJOrDoc: "CalculateDoj",
        LvCalculationOnDOJ: null,
        LvCalculationOnDOC: null,
        LvAvailedOnDOJorDoc: 'CalAvailDoj',
        LvAvailedOnFixedOrPercentage: "Percentage",
        LvCanAvailQuantity: 100,
        LvAvailedOnDOJ: null,
        LvAvailedOnDOC: null,
        LvCanAvailAfter: 0,
        CanAvailUOM: null,
        IsCFFixed: 'true',
        IsCFRestFixed: false,
        IsCFCRestFixed: false,
        IsCFRestEncash: 'true',
        IsCFCRestEncash: 'true',
        IsProrataMonthly: 'true',
        CarryForwardRoundupOption: null,
        EncashmentBasis: 'CalanderYear',
        LvEncashmentFormulaDesID: null,
        FormulaDescription: null,
        EncashmentDate: null

    };

    $scope.leaveTypelist = [];
    $scope.GetCbo = function () {
        $http.get('Leave/LeavePolicy/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.leaveTypelist = [];
                        $scope.leaveTypelist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

    $scope.LeavePolicyList = [];
    $scope.getListData = function () {
        $http.post('Leave/LeavePolicy/getlist?PlantID=' + $scope.LeavePolicyModel.PlantID)
            .then(
                function successCallback(response) {
                    $scope.LeavePolicyList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.LeavePolicyList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    //$scope.getListData();

    $scope.LeaveDayTypeList = [];
    $scope.getDayTypeData = function () {
        $http.get('Leave/LeavePolicy/getDayTypeDatalist?SystemID=' + $scope.LeavePolicyModelDetails.SystemID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.LeaveDayTypeList = [];
                        $scope.LeaveDayTypeList = response.data;
                        var gridObj = $("#GridDayType").data("ejGrid");
                        gridObj.refreshContent(true);
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getDayTypeData();

    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.EarnLeaveVar = false;
            $scope.ActionDetails = 'SaveDetails';

            if (baseService.isUndefinedOrNull($scope.LeavePolicyModel.PolicyCode)) {
                throw 'Leave policy master is not created.';
            }

            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral = {};

            $scope.LeavePolicyModelDetails = {
                SystemID: null,
                LPMSystemID: null,
                LTSystemID: null,
                IsWithoutPay: null,
                IsWithoutPay: 0,
                IsCarryForward: null,
                CarryForwardDay: 0,
                IsMaxAllocation: null,
                MaxAllocationLimit: 0,
                MinAllocationLimit: 0,
                CalculationBasis: null,
                LeaveCredit: null,
                IsExcessAllow: false,
                IsPrecedingWeekoff: false,
                IsSucceedignHoliday: false,
                IsSucceedignWeekoff: false,
                IsSucceedignHoliday: false,
                InBetweenWeekoff: false,
                InBetweenHoliday: false,
                LeaveInHourDaily: false,
                IsActive: false,
                GroupID: null,
                PlantID: null,
                IsMaxEncashmentLapse: false,
                MaxEncashment: 0,
                IsMaxEncashmentLapse: false,
                MaxEncashmentLapse: 0,
                IsAllowed: 'true',
                IsAllowedonspecialappeal: false,
                IsProratacurrentyear: false,
                IsAsperEntryOnW: true,
                IsAsperEntryOnH: true,
                IsNoLeaveOnW: false,
                IsNoLeaveOnH: false,
                IsNewlyJoined: false,
                NewlyJoined: 0,
                EncashWorkingDaysQty: 0,
                EncashEarnLeaveQty: 0,
                IsSubmittoApproval: false,
                IsAvailPreviousYearProRata: false,
                IsAvailCurrentYearProRata: false,
                IsAvailExceptionAllowedOnSpecialAppeal: false,
                AllowedAfterDays: 0,
                IsPostApplicationAllowed: false,
                IsExceptionAllowed: false,
                IsSubjectToApproval: false,
                IsProofDocRequired: false,
                ProofDocReqAfterDays: 0,
                LvCalculationOnDOJOrDoc: "CalculateDoj",
                LvCalculationOnDOJ: null,
                LvCalculationOnDOC: null,
                LvAvailedOnDOJorDoc: 'CalAvailDoj',
                LvAvailedOnFixedOrPercentage: "Percentage",
                LvCanAvailQuantity: 100,
                LvAvailedOnDOJ: null,
                LvAvailedOnDOC: null,
                LvCanAvailAfter: 0,
                CanAvailUOM: null,
                IsCFFixed: 'true',
                IsCFRestFixed: false,
                IsCFCRestFixed: false,
                IsCFRestEncash: 'true',
                IsCFCRestEncash: 'true',
                IsProrataMonthly: 'true',
                CarryForwardRoundupOption: null,
                EncashmentBasis: 'CalanderYear',
                LvEncashmentFormulaDesID: null,
                FormulaDescription: null,
                EncashmentDate: null,
                LeaveCalculationRoundOption: null,
                IsBackDatePosting: false,
                BackDatePostingAllowedDays: 0,
                EmpCatId: null
            };

            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            $scope.getDayTypeData();
        } catch (e) {
            ShowResult(e, "failure");
        }

    }

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridLeavePolicy").data("ejGrid");
        $scope.LeavePolicyModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {

        }
        $scope.getListDataDetails();
    };

    $scope.recorddoubleclickDetails = function () {

        var gridObj = $("#DetailsABC").data("ejGrid");
        $scope.LeavePolicyModelDetails = gridObj.getSelectedRecords()[0];

        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            $scope.ActionDetails = 'UpdateDetails';
            $scope.getDayTypeData();
            $scope.getListDataDetails();
            if ($scope.LeavePolicyModelDetails.UserName == 'Earned Leave') {
                $scope.EarnLeaveVar = true;

                $scope.salaryRuleGeneral.FormulaDescription = $scope.LeavePolicyModelDetails.FormulaDescription;
                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.LeavePolicyModelDetails.LvEncashmentFormulaDesID;

            } else {
                $scope.EarnLeaveVar = false;

            }
            $scope.ShowEarnLeavePolicy();
        } catch (e) {

        }
    };

    function validation() {
        try {
            if (baseService.isUndefinedOrNull($scope.LeavePolicyModel.PolicyCode.length > 30)) {
                throw 'CODE Max allowed Length 30...';
            }
        }
        catch (e) {
            throw e;
        }
    }

    $scope.MasterId = null;
    $scope.Save = function () {
        try {


            validation();
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.leavePolicyForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.LeavePolicyModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.MasterId = response.data.MasterId;
                            $scope.LeavePolicyModel.SystemID = response.data.MasterId;
                            $scope.Action = 'Update';
                            $scope.Clear();
                            $scope.getListData();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.LeavePolicyModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.MasterId = response.data.MasterId;
                            $scope.LeavePolicyModel.SystemID = response.data.MasterId;
                            $scope.Action = 'Update';
                            $scope.Clear();
                            $scope.getListData();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.LeavePolicyDetailsList = [];
    $scope.getListDataDetails = function () {
        $scope.LeavePolicyDetailsList = [];
        $http.get('Leave/LeavePolicy/getdetailslist?MasterId=' + $scope.LeavePolicyModel.SystemID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.LeavePolicyDetailsList = response.data;

                        $scope.salaryRuleGeneral.FormulaDescription = $scope.LeavePolicyModelDetails.FormulaDescription;
                        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.LeavePolicyModelDetails.LvEncashmentFormulaDesID;

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SaveDetails = function () {
        try {

            if ($scope.LeavePolicyModelDetails.InBetweenWeekoff == true) {
                $scope.LeavePolicyModelDetails.IsPrecedingWeekoff = true;
                $scope.LeavePolicyModelDetails.IsSucceedignWeekoff = true;
            }
            else {
                $scope.LeavePolicyModelDetails.IsPrecedingWeekoff = false;
                $scope.LeavePolicyModelDetails.IsSucceedignWeekoff = false;
            }

            if ($scope.LeavePolicyModelDetails.InBetweenHoliday == true) {
                $scope.LeavePolicyModelDetails.IsSucceedignHoliday = true;
                $scope.LeavePolicyModelDetails.IsPrecedingHoliday = true;
            } else {
                $scope.LeavePolicyModelDetails.IsSucceedignHoliday = false;
                $scope.LeavePolicyModelDetails.IsPrecedingHoliday = false;
            }

            $scope.LeavePolicyModelDetails.FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.LeavePolicyModelDetails.LvEncashmentFormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
            $scope.LeavePolicyModelDetails.PlantID = $scope.LeavePolicyModel.PlantID;
            ValidationDetails();
            var LeaveDayTypeListNew = [];
            for (var i = 0; i < $scope.LeaveDayTypeList.length; i++) {
                if ($scope.LeaveDayTypeList[i].Active == true) {
                    LeaveDayTypeListNew.push(Object.assign({}, $scope.LeaveDayTypeList[i]));
                }
            }
            if ($scope.LeavePolicyModelDetails.LvCalculationOnDOJOrDoc === 'CalculateDoj') {
                $scope.LeavePolicyModelDetails.LvCalculationOnDOJ = true;
                $scope.LeavePolicyModelDetails.LvCalculationOnDOC = false;
            }

            if ($scope.LeavePolicyModelDetails.LvCalculationOnDOJOrDoc === 'CalculateDoc') {
                $scope.LeavePolicyModelDetails.LvCalculationOnDOC = true;
                $scope.LeavePolicyModelDetails.LvCalculationOnDOJ = false;
            }

            if ($scope.LeavePolicyModelDetails.LvAvailedOnDOJorDoc === 'CalAvailDoj') {
                $scope.LeavePolicyModelDetails.LvAvailedOnDOJ = true;
                $scope.LeavePolicyModelDetails.LvAvailedOnDOC = false;
            }

            if ($scope.LeavePolicyModelDetails.LvAvailedOnDOJorDoc === 'CalAvailDoc') {
                $scope.LeavePolicyModelDetails.LvAvailedOnDOC = true;
                $scope.LeavePolicyModelDetails.LvAvailedOnDOJ = false;
            }

            if ($scope.LeavePolicyModelDetails.IsProratacurrentyear === false) {
                $scope.LeavePolicyModelDetails.IsProrataMonthly = false;
            }

            if ($scope.LeavePolicyModelDetails.EncashmentBasis === 'CalanderYear') {
                $scope.LeavePolicyModelDetails.EncashmentBasis = 'CalanderYear';
                $scope.LeavePolicyModelDetails.EncashmentSpecificMonth = null;
                $scope.LeavePolicyModelDetails.EncashmentSpecificDay = null;
            }

            if ($scope.LeavePolicyModelDetails.EncashmentBasis === 'DOJ') {
                $scope.LeavePolicyModelDetails.EncashmentBasis = 'DOJ';
                $scope.LeavePolicyModelDetails.EncashmentSpecificMonth = null;
                $scope.LeavePolicyModelDetails.EncashmentSpecificDay = null;
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.leavePolicyForm.$valid) {
                if ($scope.ActionDetails === 'SaveDetails') {

                    $http({
                        method: 'POST',
                        url: $scope.saveDetailsUrl,
                        data: { 'LeavePolicyDetails': $scope.LeavePolicyModelDetails, 'MasterId': $scope.LeavePolicyModel.SystemID, 'leavePolicyDayType': LeaveDayTypeListNew },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.LeavePolicyModelDetails = {};
                            $scope.getDayTypeData();
                            $scope.getListDataDetails();
                            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                            eDialog.close();
                            if (!$rootScope.isCollapsed) {
                                $rootScope.toggle();
                            }
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.ActionDetails === 'UpdateDetails') {

                    $http({
                        method: 'POST',
                        url: $scope.updateDetailsUrl,
                        data: { 'LeavePolicyDetails': $scope.LeavePolicyModelDetails, 'MasterId': $scope.LeavePolicyModel.SystemID, 'leavePolicyDayType': LeaveDayTypeListNew },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.LeavePolicyModelDetails = {};
                            $scope.getDayTypeData();

                            $scope.getListDataDetails();

                            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                            eDialog.close();

                            if (!$rootScope.isCollapsed) {
                                $rootScope.toggle();
                            }

                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.LeavePolicyModel.SystemID)) {

            $http.get('Leave/LeavePolicy/Delete?SystemID=' + $scope.LeavePolicyModel.SystemID)

                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.LeavePolicyModel = {};
                        $scope.Clear();
                        $scope.getListData();
                        var gridObj = $("#GridLeavePolicy").data("ejGrid");
                        gridObj.refreshContent();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });

        }
    };

    $scope.DeleteDetails = function () {
        if (!baseService.isUndefinedOrNull($scope.LeavePolicyModelDetails.SystemID)) {

            $http.get('Leave/LeavePolicy/DeleteDetails?SystemID=' + $scope.LeavePolicyModelDetails.SystemID)

                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.Clear();
                        $scope.LeavePolicyModelDetails = {};

                        $scope.getDayTypeData();
                        $scope.getListData();

                        $scope.getListDataDetails();

                        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                        eDialog.close();

                        if (!$rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }


                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function (obj) {
        ClearFields(obj);
    };

    function ClearFields(obj) {
        $scope.Action = 'Save';

        $scope.PlantID = $scope.LeavePolicyModel.PlantID;
        $scope.CompanyId = $scope.LeavePolicyModel.CompanyId;

        for (var i in obj) {
            obj[i] = null;
        }
        $scope.LeavePolicyDetailsList = [];
        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral = [];
        $scope.LeavePolicyModel.PlantID = $scope.PlantID;
        $scope.LeavePolicyModel.CompanyId = $scope.CompanyId;
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Policy Code", $scope.LeavePolicyModel.PolicyCode);
            CheckField("Policy Name", $scope.LeavePolicyModel.PolicyName);

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationDetails() {
        try {
            CheckField("Leave Type", $scope.LeavePolicyModelDetails.LTSystemID);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.changeVlueCarryForward = function () {
        $scope.LeavePolicyModelDetails.CarryForwardDay = null;
    };

    $scope.changeVlueCarryForwardEncashment = function () {
        $scope.LeavePolicyModelDetails.MaxEncashment = null;
    };

    $scope.ChangeIsAllowed = function () {
        $scope.LeavePolicyModelDetails.AllowedAfterDays = null;
    };

    $scope.ChangeProofDocRequired = function () {
        $scope.LeavePolicyModelDetails.ProofDocReqAfterDays = null;
    };

    $scope.ChangeFixed = function () {
        $scope.LeavePolicyModelDetails.CarryForwardRoundupOption = null;
    };

    $scope.ChangeAllow = function () {
        $scope.LeavePolicyModelDetails.IsAllowedonspecialappeal = null;
    };

    $scope.changeexceptionallowed = function () {
        $scope.LeavePolicyModelDetails.IsSubjectToApproval = null;
    };

    $scope.ChangeEncasementDate = function () {
        $scope.LeavePolicyModelDetails.EncashmentSpecificDay = null;
        $scope.LeavePolicyModelDetails.EncashmentSpecificMonth = null;
    };

    $scope.LeaveType = null;
    $scope.ShowEarnLeavePolicy = function () {
        $scope.LT = $("#LeaveTypeID option:selected").text();

        $scope.LeaveType = $.grep($scope.leaveTypelist, function (item) {
            return item.Id === $scope.LeavePolicyModelDetails.LTSystemID;
        })[0].LeaveType;

        if ($scope.LeaveType == 'Earn') {
            $scope.EarnLeaveVar = true;
            $scope.LeavePolicyModelDetails.IsProratacurrentyear = false;
        }
        else {
            $scope.EarnLeaveVar = false;
        }
    }

    $scope.SalaryHeadlist = [];
    $scope.GetSalaryHead = function () {
        $http.get('Leave/LeavePolicy/GetSalaryHeadCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SalaryHeadlist = [];
                        $scope.SalaryHeadlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetSalaryHead();

    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.salaryRuleGeneral = {
        FormulaDescription: null,
        FormulaIDDescription: null
    };

    $scope.SetFormula = function (formula) {
        if (formula === 'SHead') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.SalaryHeadIdFormula)) {
                $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.SalaryHeadFormula;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
            }

            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Operator') {
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Operator)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Operator;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Operator;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Precedence') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Precedence)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Precedence;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Precedence;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Value') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Value)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Value;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Value;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
    };

    $scope.RemoveFormula = function () {
        $scope.salaryRuleGeneral.FormulaDesID = null;

        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);

        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);

        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDes += $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                $scope.salaryRuleGeneral.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    };

    // #region Radio button Value for Week OFF
    $scope.radiovalue = false;
    $scope.InBetweenWeekoff = false;
    $scope.IsNoLeaveOnW = false;
    $scope.IsAsperEntryOnW = true;
    $scope.setRadioInBetweenWeekoff = function () {
        $scope.radiovalue = true;
        $scope.InBetweenWeekoff = true;
        $scope.IsNoLeaveOnW = false;
        $scope.IsAsperEntryOnW = false;
        $scope.LeavePolicyModelDetails.InBetweenWeekoff = true;
        $scope.LeavePolicyModelDetails.IsAsperEntryOnW = false;
        $scope.LeavePolicyModelDetails.IsNoLeaveOnW = false;
    }
    $scope.setRadioIsAsperEntryOnW = function () {
        $scope.radiovalue = true;
        $scope.IsAsperEntryOnW = true;
        $scope.InBetweenWeekoff = false;
        $scope.IsNoLeaveOnW = false;

        $scope.LeavePolicyModelDetails.InBetweenWeekoff = false;
        $scope.LeavePolicyModelDetails.IsAsperEntryOnW = true;
        $scope.LeavePolicyModelDetails.IsNoLeaveOnW = false;
    }
    $scope.setRadioIsNoLeaveOnW = function () {
        $scope.radiovalue = true;
        $scope.InBetweenWeekoff = false;
        $scope.IsAsperEntryOnW = false;
        $scope.IsNoLeaveOnW = true;
        $scope.LeavePolicyModelDetails.InBetweenWeekoff = false;
        $scope.LeavePolicyModelDetails.IsAsperEntryOnW = false;
        $scope.LeavePolicyModelDetails.IsNoLeaveOnW = true;
    }
    // #endregion Radio button Value for Week OFF

    // #region Radio button Value for Holiday
    $scope.radiovalu = false;
    $scope.InBetweenHoliday = false;
    $scope.IsNoLeaveOnH = false;
    $scope.IsAsperEntryOnH = true;
    $scope.setRadioInBetweenHoliday = function () {
        $scope.radiovalu = true;
        $scope.InBetweenHoliday = true;
        $scope.IsNoLeaveOnH = false;
        $scope.IsAsperEntryOnH = false;
        $scope.LeavePolicyModelDetails.InBetweenHoliday = true;
        $scope.LeavePolicyModelDetails.IsAsperEntryOnH = false;
        $scope.LeavePolicyModelDetails.IsNoLeaveOnH = false;
    }
    $scope.setRadioIsAsperEntryOnH = function () {
        $scope.radiovalu = true;
        $scope.IsAsperEntryOnH = true;
        $scope.InBetweenHoliday = false;
        $scope.IsNoLeaveOnH = false;

        $scope.LeavePolicyModelDetails.InBetweenHoliday = false;
        $scope.LeavePolicyModelDetails.IsAsperEntryOnH = true;
        $scope.LeavePolicyModelDetails.IsNoLeaveOnH = false;
    }
    $scope.setRadioIsNoLeaveOnH = function () {
        $scope.radiovalu = true;
        $scope.InBetweenHoliday = false;
        $scope.IsAsperEntryOnH = false;
        $scope.IsNoLeaveOnH = true;
        $scope.LeavePolicyModelDetails.InBetweenHoliday = false;
        $scope.LeavePolicyModelDetails.IsAsperEntryOnH = false;
        $scope.LeavePolicyModelDetails.IsNoLeaveOnH = true;
    }
    // #endregion Radio button Value for Holiday

    $scope.EmployeeCategoryList = [];
    $scope.getEmpCat = function () {
        $http.get('Leave/LeavePolicy/GetEmployeeCategory')
            .then(function (response) {
                $scope.EmployeeCategoryList = response.data;
            });
    };
    $scope.getEmpCat();
}