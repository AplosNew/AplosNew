'use strict';
SalesChalanController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalesChalanController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sales Chalan';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'SalesManagements/SalesChalan/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
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

    $scope.CategoryList = [
        { Value: 'GRN', Text: 'GRN' },
        { Value: 'SalesOrder', Text: 'Sales Order' },
        { Value: 'SalesInvoice', Text: 'Sales Invoice' }
    ];
    $scope.charecterTypeList = [
        { Value: 'Text', Text: "Text" },
        { Value: 'DateTime', Text: "DateTime" },
        { Value: 'Decimal', Text: "Decimal" }
    ];


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.popUpDataList = [];
    $scope.name = null;
    $scope.popUp = function (name) {
        try {
            $scope.name = name;
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

    $scope.selectdblClick = function (obj) {
        var ob = obj.data;
        if ($scope.name == 'ByWhom') {
            $scope.ModelNew.ByWhomId = ob.SystemId;
            $scope.ModelNew.ByWhom = ob.EmployeeName;
        } else if ($scope.name == 'SecurityInCharge') {
            $scope.ModelNew.SecurityInChargeId = ob.SystemId;
            $scope.ModelNew.SecurityInCharge = ob.EmployeeName;
        }
        else {
            $scope.ModelNew.ResponsiblePersonId = ob.SystemId;
            $scope.ModelNew.ResponsiblePerson = ob.EmployeeName;
        }
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.clearEmpPop = function () {
        if ($scope.name == 'ByWhom') {
            $scope.ModelNew.ByWhomId = null;
            $scope.ModelNew.ByWhom = null;
        } else if ($scope.name == 'SecurityInCharge') {
            $scope.ModelNew.SecurityInChargeId = null;
            $scope.ModelNew.SecurityInCharge = null;
        }
        else if ($scope.name == 'CheckBy') {
            $scope.ModelNew.CheckById = null;
            $scope.ModelNew.CheckBy = null;
        }
        else if ($scope.name == 'ApproveBy') {
            $scope.ModelNew.ApproveById = null;
            $scope.ModelNew.ApproveBy = null;
        }
        else {
            $scope.ModelNew.ResponsiblePersonId = null;
            $scope.ModelNew.ResponsiblePerson = null;
        }
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
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
}