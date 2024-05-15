'use strict';
serviceControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'accountService', '$window'];
function serviceControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, accountService, $window) {
    $rootScope.title = 'Service Control';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Setups/ServiceMaster/';
    $scope.getListUrl = $scope.path + 'GetServiceControlList';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateServiceControlHeader';
    $scope.saveServiceMasterUrl = $scope.path + 'CreateServiceControlServiceMaster';

    $scope.Type = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 10;
    $scope.setTab2 = function (newTab2) {
        $scope.tab2 = newTab2;
    };

    $scope.isSet2 = function (tabNum2) {
        return $scope.tab2 === tabNum2;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetServiceControlList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });

    $scope.ModelTempMat = {
        Id: null,
        UserName: null,
        StorageLocationId: null,
        StorageSubLocation: null,
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
        MaterialMasterId: null,
        MaterialMasterArticleId: null,
        AccessType: null,
        NoOfBin: null,
        Remarks: null,
        StorageLevel: null,
    };
    $scope.ModelNewMat = Object.assign({}, $scope.ModelTempMat);


    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

  

    $scope.ServiceMasterData = [];
    $scope.GetServiceMasterData = function (id) {
        $http({
            method: 'POST',
            url: 'setups/ServiceMaster/GetServiceMasterList?serviceControlId=' + id,
        }).then(function successCallback(response) {
            $scope.ServiceMasterData = response.data;
        });
        $scope.ServiceControlId = id;
    }
    $scope.refreshTemplateServiceMaster = function (args) {
        $("#serviceMasterheadchk").ejCheckBox({ "change": CheckBoxSelectAllServiceMaster });
    };
    function CheckBoxSelectAllServiceMaster(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridService").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ServiceMasterData.length; i++) {
                $scope.ServiceMasterData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridService").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetServiceMasterData(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.ServiceControlId = args.data.Id;
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm2.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.ServiceControlId = response.data.Data.Id;
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.GlControlId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.GlControlId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                    $scope.selectExpenseGL();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.EmployeeCategoryList = [];
        $scope.employee = [];
        $scope.DesignationList = [];
        $scope.PositionCodeListData = [];
        $scope.BudgetCodeList = [];
        $scope.EmployeeList = [];
        $scope.ControlDrListData = [];
        $scope.ControlCrListData = [];
        $scope.ActionByListData = [];
        $scope.ApproveByListData = [];
        $scope.ResponsiblePersonListData = [];
    }

   

    $scope.SaveServiceMaster = function () {
        if (baseService.isUndefinedOrNull($scope.ServiceControlId)) {
            return ShowResult('Please select Service ControlId!', 'failure');
        }
        $scope.tempServiceMasterList = [];
        for (var i = 0; i < $scope.ServiceMasterData.length; i++) {
            if ($scope.ServiceMasterData[i].CheckBoxSelect == true) {
                $scope.tempServiceMasterList.push($scope.ServiceMasterData[i]);
            }
            if ($scope.ServiceMasterData[i].CheckBoxSelect == false && $scope.ServiceMasterData[i].Id != null) {
                $scope.tempServiceMasterList.push($scope.ServiceMasterData[i]);
            }
        }

        $http({
            method: 'POST',
            url: $scope.saveServiceMasterUrl,
            data: { 'data': $scope.tempServiceMasterList, 'serviceControlId': $scope.ServiceControlId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetServiceMasterData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    //#region Action By
    $scope.ActionByListData = [];
    $scope.getActionByPopUpData = function () {
        $http({
            method: 'POST',
            url: 'Setups/ServiceMaster/GetServiceActionByList?serviceControlId=' + $scope.ServiceControlId
        }).then(function successCallback(response) {
            $scope.ActionByListData = response.data;
        });
    }

    $scope.ActionByList = [];
    $scope.OKActionBy = function () {
        $scope.ActionByList = [];
        try {
            for (var i = 0; i < $scope.ActionByListData.length; i++) {
                if ($scope.ActionByListData[i].CheckBoxSelect == true) {
                    $scope.ActionByList.push($scope.ActionByListData[i]);
                }
                if ($scope.ActionByListData[i].CheckBoxSelect == false && $scope.ActionByListData[i].Id != null) {
                    $scope.ActionByList.push($scope.ActionByListData[i]);
                }
            }

            $scope.SaveABData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.refreshTemplateAB = function (args) {
        $("#ABheadchk").ejCheckBox({ "change": CheckBoxSelectAllAB });
    };

    function CheckBoxSelectAllAB(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridAB").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ActionByListData.length; i++) {
                $scope.ActionByListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAB").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveABData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ActionByList.length; i++) {
            for (var j = 0; j < $scope.ActionByListData.length; j++) {
                if ($scope.ActionByListData[j].SystemID == $scope.ActionByList[i].ActionById) {
                    if ($scope.ActionByListData[j].CheckBoxSelect == false) {
                        $scope.ActionByList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveABUrl,
            data: { 'data': $scope.ActionByList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getActionByPopUpData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Action By


    //#region Approve By
    $scope.ApproveByListData = [];
    $scope.getApproveByPopUpData = function () {
        $http({
            method: 'Post',
            url: 'Setups/ServiceMaster/GetServiceApprovedByList?serviceControlId=' + $scope.ServiceControlId
        }).then(function successCallback(response) {
            $scope.ApproveByListData = response.data;
        });
    }

    $scope.ApproveByList = [];
    $scope.OKApproveBy = function () {
        $scope.ApproveByList = [];
        try {
            for (var i = 0; i < $scope.ApproveByListData.length; i++) {
                if ($scope.ApproveByListData[i].CheckBoxSelect == true) {
                    $scope.ApproveByList.push($scope.ApproveByListData[i]);
                }
                if ($scope.ApproveByListData[i].CheckBoxSelect == false && $scope.ApproveByListData[i].Id != null) {
                    $scope.ApproveByList.push($scope.ApproveByListData[i]);
                }
            }
            $scope.SaveAPBData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateAPB = function (args) {
        $("#APBheadchk").ejCheckBox({ "change": CheckBoxSelectAllAPB });
    };

    function CheckBoxSelectAllAPB(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridAPB").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ApproveByListData.length; i++) {
                $scope.ApproveByListData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAPB").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveAPBData = function () {
        if (baseService.isUndefinedOrNull($scope.GlManagementId)) {
            return ShowResult('Please select GL Management!', 'failure');
        }
        for (var i = 0; i < $scope.ApproveByList.length; i++) {
            for (var j = 0; j < $scope.ApproveByListData.length; j++) {
                if ($scope.ApproveByListData[j].SystemID == $scope.ApproveByList[i].ApproveById) {
                    if ($scope.ApproveByListData[j].CheckBoxSelect == false) {
                        $scope.ApproveByList[i].CheckBoxSelect = false;
                    }
                }
            }
        }
        $http({
            method: 'POST',
            url: $scope.saveAPBUrl,
            data: { 'data': $scope.ApproveByList, 'GlManagementId': $scope.GlManagementId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getApproveByPopUpData($scope.GlManagementId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    //#endregion Approve By
}