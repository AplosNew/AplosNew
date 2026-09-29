'use strict';
SKUResponsiblePersonController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function SKUResponsiblePersonController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "SKU Responsible Person";
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Status', name: "Status" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/MasterOrder/GetSKUResPersonList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Status: 'Active',
        ResponsiblePersonId: null,
        TargetDate: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.StatusList = [
        {
            'Text': 'Active',
            'Value': 'Active'
        },
        {
            'Text': 'Running',
            'Value': 'Running'
        },
        {
            'Text': 'Closed',
            'Value': 'Closed'
        }
    ];

    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function () {
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


    $scope.setEmpData = function (obj) {
        $scope.ModelNew.ResponsiblePersonId = obj.data.SystemId;
        $scope.ModelNew.ResponsiblePersonName = obj.data.EmployeeName;
        angular.element(document.querySelector('#popUp')).modal('hide');
    };


    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };


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
                url: "OrderManagements/MasterOrder/CreateSKUResPerson",
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
    $scope.deleteUrl = "OrderManagements/MasterOrder/DeleteSKUResPerson/"
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

