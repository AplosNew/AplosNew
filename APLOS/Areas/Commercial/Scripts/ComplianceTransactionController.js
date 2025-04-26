'use strict';
ComplianceTransactionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance Transaction';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.ComplianceList = [];
    $scope.path = 'Commercial/Compliance/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.searchBy = "Code"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'Remarks', name: "Remarks" }];

    $scope.getComplianceData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ComplianceList = response.data;
        });
    }
    $scope.getComplianceData();

    $scope.ModelTemp = {
        Id: null,
        ComplianceGroup: null,
        Code: null,
        Category: null,
        SubCategory: null,
        ItemName: null,
        CriticalityLevel: null,
        ComplianceValue: null,
        Remarks: null,
        LocationReference: null,
        ScanApplicable: null,
        CodeApplicable: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

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
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.popUpDataList = [];
    $scope.name = null;
    $scope.popUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SetEmployee = function (args) {
        $scope.ModelNew.EmployeeId = args.data.SystemId;
        $scope.ModelNew.EmployeeCode = args.data.EmployeeCode;
        $scope.ModelNew.EmployeeName = args.data.EmployeeName;
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };


}