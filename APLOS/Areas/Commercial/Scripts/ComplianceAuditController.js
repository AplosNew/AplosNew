'use strict';
ComplianceAuditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceAuditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance Audit';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.ComplianceList = [];
    $scope.path = 'Commercial/Compliance/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateTransaction';
    $scope.deleteUrl = $scope.path + 'DeleteTransaction/';
    $scope.Action = 'Save';
    $scope.searchBy = "Code"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'Remarks', name: "Remarks" }];

    $scope.Grouplist = [];
    $scope.GetGroupCbo = function () {
        $http.get('Commercial/Compliance/GetGroupCbo')
            .then(function (response) {
                $scope.Grouplist = response.data;
            });
    };
    $scope.GetGroupCbo();

    $scope.CategoryList = [];
    $scope.GetCategoryCbo = function () {
        $http.get('Commercial/Compliance/GetCategoryCbo')
            .then(function (response) {
                $scope.CategoryList = response.data;
            });
    };
    $scope.GetCategoryCbo();

    $scope.SubCategoryList = [];
    $scope.GetSubCategoryCbo = function () {
        $http.get('Commercial/Compliance/GetSubCategoryCbo')
            .then(function (response) {
                $scope.SubCategoryList = response.data;
            });
    };
    $scope.GetSubCategoryCbo();

    $scope.getComplianceData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetComplianceDataList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ComplianceList = response.data;
        });
    }
    $scope.getComplianceData();

    $scope.CriticalityLevelList = [
        { 'Value': "Normal", 'Text': "Normal" },
        { 'Value': "Critical", 'Text': "Critical" },
        { 'Value': "Important", 'Text': "Important" }
    ];

    $scope.auditFrequencyUnitList = [
        { 'Value': "Days", 'Text': "Days" },
        { 'Value': "Hour", 'Text': "Hour" }
    ];

    $scope.ComplianceValueList = [
        { 'Value': "0", 'Text': "0" },
        { 'Value': "1", 'Text': "1" },
        { 'Value': "2", 'Text': "2" },
        { 'Value': "3", 'Text': "3" },
        { 'Value': "4", 'Text': "4" }
    ];


    $scope.ScorePointList = [
        { 'Value': "1", 'Text': "1" },
        { 'Value': "2", 'Text': "2" },
        { 'Value': "3", 'Text': "3" },
        { 'Value': "4", 'Text': "4" }
    ];


    $scope.CheckMarkList = [
        { 'Value': "1", 'Text': "Yes" },
        { 'Value': "0", 'Text': "No" }
    ];

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.ComplianceValue = $scope.ModelNew.ComplianceValue.toString();
        $("#CreateNewPopUp").data("ejDialog").open();


    };

    $scope.SourceType = "";

    $scope.Go = function () {
        try {
            if ($scope.ModelNew.RPACount == 2) {
                if (baseService.isUndefinedOrNull($scope.SourceType)) {
                    throw "Select Responsible Person or Auditor.";
                }
                $scope.GetCheckPointsList();
                $scope.closePopup('CreateNewPopUp');
                $scope.Action = 'Update';
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            } else {
                $scope.GetCheckPointsList();
                $scope.closePopup('CreateNewPopUp');
                $scope.Action = 'Update';
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }

    $scope.ShowpopUp = function () {
        if (!baseService.isUndefinedOrNull($scope.SubCategoryModelNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.SubCategoryModelNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmSCDataPopUp')).modal('show');
    }

    $scope.CheckPList = [];
    $scope.GetCheckPointsList = function () {
        $scope.CheckPList = [];
        $http.get('Commercial/Compliance/GetComplianceCheckPointsData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                $scope.CheckPList = response.data;
                for (var i = 0; i < $scope.CheckPList.length; i++) {
                    $scope.CheckPList[i].CheckMark = null;
                }
            });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
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
                    ClearFields();
                    $scope.getCTData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


}