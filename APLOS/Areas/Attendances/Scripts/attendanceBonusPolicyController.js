'use strict';
attendanceBonusPolicyController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function attendanceBonusPolicyController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Attendance Bonus Policy';
    $scope.path = 'Attendances/AttendanceBonusPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveLeaveUrl = $scope.path + 'SaveLeave';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'DeleteDetails/';

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#AttendanceBonusD").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();

                $("#AttendanceBonusD").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };

    $scope.plantList = [];
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.AttendanceBPMaster.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.ModelList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetList?plantid=" + $scope.AttendanceBPMaster.PlantID,
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            
        });
    }

    
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#EmpGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.dataList.length; i++) {
                $scope.dataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#EmpGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.AttendanceBPMaster = {
        Id: null,
        AttenBnsPolicyName: null,
        AttenBnsPolicyDescription: null,
        MID: null,
        PlantID: null,
        GroupID: null,
        CompanyId: null,
    };

    $scope.OpenAdditionalPolicyDialog = function () {
        $scope.getLeaveTypeList();
        var eDialog = $("#dialogPFSetting").data("ejDialog");
        eDialog.open();
    };

    $scope.dataList = [];
    $scope.getLeaveTypeList = function () {
        $http.get('Attendances/AttendanceBonusPolicy/GetLeaveList?AttdnBonusPmtPolicyDetailsId=' + $scope.AttendanceBP.ID)
            .then(function (response) {
                $scope.dataList = response.data;
            });
    };
   
    $scope.AttendanceBP = {
        ID: null,
        FixedOrFormula: 'Fixed',
        FixedValue: 500,
        MaxEarlyOutAllowed: null,
        FormulaDes: null,
        FormulaDesID: null,
        MID: null,

        IsLateInApplicable: true,
        IsEarlyOutApplicable: true,
        IsLunchOutApplicable: true,
        IsAbsentApplicable: true,
        IsLateApplicable: true,
        IsLeaveApplicable: true,
        IsLeaveWithOutPayApplicable: true,
        IsRouteApplicableForLate: true,

        EOLIFromValue: 0,
        EOLIToValue: 3,
        LunchOutFromValue: 0,
        LunchOutToValue: 0,
        AbsentFromValue: 0,
        AbsentToValue: 0,
        LateFromValue: 0,
        LateToValue: 3,
        LeaveFromValue: 0,
        LeaveToValue: 31,
        LeaveWithOutPayFromValue: 0,
        LeaveWithOutPayToValue: 0,
    };

    $scope.AttendanceBPModel = Object.assign({}, $scope.AttendanceBP);

    $scope.AttendanceBonusDetailsList = [];
    $scope.getDetails = function () {
        $http.get('Attendances/AttendanceBonusPolicy/GetDetailsList?MasterId=' + $scope.AttendanceBPMaster.MID)
            .then(function (response) {
                $scope.AttendanceBonusDetailsList = response.data;

            });
    };

    $scope.SaveLeaveType = function () {
        try {
            var NewdataList = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelect == true) {
                    NewdataList.push($scope.dataList[i]);
                }
            }

            if (NewdataList.length == 0) {
                throw "Please Select LeaveType";
            }

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveLeaveUrl,
                data: { 'LeaveList': NewdataList, 'MasterId': $scope.AttendanceBPMaster.MID, 'DetailsId': $scope.AttendanceBP.ID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DetailsId = null;
    $scope.Save = function () {
        try {
            var NewdataList = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelect == true) {
                    NewdataList.push($scope.dataList[i]);
                }
            }

            if ($scope.AttendanceBP.FixedValue < 0) {
                throw 'Fixed Value Can not below then 0';
            }

            $scope.AttendanceBP.FormulaDes = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.AttendanceBP.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
            $scope.AttendanceBP.MID = $scope.AttendanceBPMaster.MID;

            if ($scope.AttendanceBP.FixedOrFormula == 'Fixed') {
                $scope.AttendanceBP.FormulaDesID = null;
                $scope.AttendanceBP.FormulaDes = null;
            }

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'Details': $scope.AttendanceBP, 'LeaveList': NewdataList, 'MasterId': $scope.AttendanceBPMaster.MID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.AttendanceBP.ID = response.data.DetailsId;
                    $scope.Clear();
                    $scope.getMaster();
                    $scope.getDetails();
                    $scope.ConfirmrebateClose();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.ConfirmrebateClose = function () {
        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
        eDialog.close();
    };
    $scope.MasterId = null;
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveMUrl,
                data: { 'Master': $scope.AttendanceBPMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AttendanceBP.MID = response.data.MasterId;
                    $scope.AttendanceBPMaster.MID = response.data.MasterId;
                    $scope.getMaster();
                  
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridShiftTimeChange").data("ejGrid");
        $scope.AttendanceBPMaster = gridObj.getSelectedRecords()[0];
        $scope.AttendanceBP.MID = $scope.AttendanceBPMaster.MID;
        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
        }
        $scope.getDetails($scope.AttendanceBPMaster.MID);
    };

    $scope.recorddoubleclickDetails = function () {
        $scope.dataList = [];
        var gridObj = $("#AttendanceBonusD").data("ejGrid");
        $scope.AttendanceBP = gridObj.getSelectedRecords()[0];

        $scope.salaryRuleGeneral.FormulaDescription = $scope.AttendanceBP.FormulaDes;
        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.AttendanceBP.FormulaDesID;

        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            $scope.getLeaveTypeList();

        } catch (e) {

        }
    };

    $scope.DeleteMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.AttendanceBPMaster.MID)) {
            $http.get('Attendances/AttendanceBonusPolicy/DeleteM?SystemID=' + $scope.AttendanceBPMaster.MID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearM();
                        $scope.Clear();
                        $scope.getMaster();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteDetailsFunction = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: { 'DetailsId': $scope.AttendanceBP.ID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');                
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getDetails();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClearM = function () {
        ClearFields();
        return true;
    };

    function ClearFields(obj) {        
        $scope.AttendanceBPMaster = {
            Id: null,
            AttenBnsPolicyName: null,
            AttenBnsPolicyDescription: null,
            MID: null,
            PlantID: $scope.AttendanceBPMaster.PlantID,
            GroupID: null,
            CompanyId: $scope.AttendanceBPMaster.CompanyId,
        };
        $scope.getMaster();
        $scope.AttendanceBonusDetailsList = [];

    }

    $scope.Clear = function () {
        ClearField();
        return true;
    };

    function ClearField() {
        $scope.AttendanceBP = {
            ID: null,
            FixedOrFormula: 'Fixed',
            FixedValue: 500,
            MaxEarlyOutAllowed: null,
            FormulaDes: null,
            FormulaDesID: null,
            MID: null,

            IsLateInApplicable: true,
            IsEarlyOutApplicable: true,
            IsLunchOutApplicable: true,
            IsAbsentApplicable: true,
            IsLateApplicable: true,
            IsLeaveApplicable: true,
            IsLeaveWithOutPayApplicable: true,
            IsRouteApplicableForLate: true,

            EOLIFromValue: 0,
            EOLIToValue: 3,
            LunchOutFromValue: 0,
            LunchOutToValue: 0,
            AbsentFromValue: 0,
            AbsentToValue: 0,
            LateFromValue: 0,
            LateToValue: 3,
            LeaveFromValue: 0,
            LeaveToValue: 31,
            LeaveWithOutPayFromValue: 0,
            LeaveWithOutPayToValue: 0,
        };
        $scope.dataList = [];
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("AttenBns Policy Name", $scope.AttendanceBPMaster.AttenBnsPolicyName);
            CheckField("AttenBns Policy Description", $scope.AttendanceBPMaster.AttenBnsPolicyDescription);

        } catch (ex) {
            throw ex;
        }
    };    

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

    $scope.ChangeEarlyOut = function () {
        $scope.AttendanceBP.EOLIFromValue = 0;
        $scope.AttendanceBP.EOLIToValue = 0;
    };
    $scope.ChangeLunchOut = function () {
        $scope.AttendanceBP.LunchOutFromValue = 0;
        $scope.AttendanceBP.LunchOutToValue = 0;
    };
    $scope.ChangeAbsent = function () {
        $scope.AttendanceBP.AbsentFromValue = 0;
        $scope.AttendanceBP.AbsentToValue = 0;
    };
    $scope.ChangeLate = function () {
        $scope.AttendanceBP.LateFromValue = 0;
        $scope.AttendanceBP.LateToValue = 0;
    };
    $scope.ChangeLeave = function () {
        $scope.AttendanceBP.LeaveFromValue = 0;
        $scope.AttendanceBP.LeaveToValue = 0;
    };
    $scope.ChangeLeaveWithOut = function () {
        $scope.AttendanceBP.LeaveWithOutPayFromValue = 0;
        $scope.AttendanceBP.LeaveWithOutPayToValue = 0;
    };

    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();

            $scope.AttendanceBP = {
                FixedOrFormula: 'Fixed',
                FixedValue: 500,
                MaxEarlyOutAllowed: null,
                FormulaDes: null,
                FormulaDesID: null,
                MID: null,

                IsLateInApplicable: true,
                IsEarlyOutApplicable: true,
                IsLunchOutApplicable: true,
                IsAbsentApplicable: true,
                IsLateApplicable: true,
                IsLeaveApplicable: true,
                IsLeaveWithOutPayApplicable: true,
                IsRouteApplicableForLate: true,

                EOLIFromValue: 0,
                EOLIToValue: 3,
                LunchOutFromValue: 0,
                LunchOutToValue: 0,
                AbsentFromValue: 0,
                AbsentToValue: 0,
                LateFromValue: 0,
                LateToValue: 3,
                LeaveFromValue: 0,
                LeaveToValue: 31,
                LeaveWithOutPayFromValue: 0,
                LeaveWithOutPayToValue: 0,
            };

        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.ChangeFixedValue = function () {
        $scope.AttendanceBP.FixedValue = null;

    };
}