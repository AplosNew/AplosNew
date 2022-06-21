'use strict';
StorageBinMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function StorageBinMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Storage Bin Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/StorageBinMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // ALL POP UPs
    $scope.OpenEmployeePopUp = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.getResponsiblePerson();
    }


    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    $scope.OpenStorageLocation = function () {

        angular.element(document.querySelector('#StorageLocationPop')).modal('show');
    }


    $scope.closeStorageLocation = function () {
        angular.element(document.querySelector('#StorageLocationPop')).modal('hide');
    }

    $scope.ModelTemp = {
        Id: null,
        StorageLocationId: null,
        StorageSubLocation: null,
        AreaRackCode: null,
        ColumnNo: null,
        RowNo: null,
        BinCode: null,
        BinReference: null,
        UserName: null,
        CapacityValue: null,
        AccessType: null,
        UserLocationType: null,
        Remarks: null,
        Active: true,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ResponsiblePerson = null;
    $scope.ResponsiblePersonId = null;
    $scope.selectResponsible = function (e) {
        $scope.ResponsiblePerson = e.data.EmployeeName;
        $scope.ResponsiblePersonId = e.data.SystemId;
        $scope.closeEmpPopUp();
    }

    $scope.ResponsiblePersonList = [];
    $scope.getResponsiblePerson = function () {
        $http({
            method: 'POST',

            url: $scope.path + 'getResponsiblePerson',
        }).then(function success(response) {
            $scope.ResponsiblePersonList = response.data;
        });
    }

    $scope.StorageLocationList = [];
    $scope.getStorageLocation = function () {
        $http({
            method: 'POST',

            url: $scope.path + 'getStorageLocation',
        }).then(function success(response) {
            $scope.StorageLocationList = response.data;
        });
    }
    $scope.getStorageLocation();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'datas': $scope.ModelNew,
                
                'ResponsiblePersonId': $scope.ResponsiblePersonId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFields();
                //$scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    // Delete Function
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    // clear Data
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields() {
        $scope.ObserveByName = null,
            $scope.ResponsiblePerson = null,
            $scope.ModelTemp = {
            Id: null,
            StorageLocationId: null,
            StorageSubLocation: null,
            AreaRackCode: null,
            ColumnNo: null,
            RowNo: null,
            BinCode: null,
            BinReference: null,
            UserName: null,
            CapacityValue: null,
            AccessType: null,
            UserLocationType: null,
            Remarks: null,
            Active: true,

            };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
}