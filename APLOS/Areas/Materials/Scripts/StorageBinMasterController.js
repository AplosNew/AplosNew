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
    $scope.searchBy = "UserName"; $scope.search = "";

    $scope.searchByList = [
        {
            value: 'Id',
            name: "Id",
        },
        {
            value: 'Code',
            name: "Code",
        },
        {
            value: 'ShortName',
            name: "Short Name",
        },
        {
            value: 'StandardName',
            name: "Standard Name",
        },
        {
            value: 'UserName',
            name: "User Name"
        },
        {
            value: 'Description',
            name: "Description",
        },
        {
            value: 'Remarks',
            name: "Remarks",
        },
        {
            value: 'ColumnNo',
            name:"ColumnNo",
        },
        {
            value: 'RowNo',
            name: "RowNo",
        },
        {
            value: 'UserLocationType',
            name: "UserLocationType",
        },
        {
            value: 'EmployeeName',
            name: "EmployeeName",
        },
    ];

    // ALL POP UPs
    $scope.OpenEmployeePopUp = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
        
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

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
            
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,      
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
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ResponsiblePerson = null;
    $scope.ResponsiblePersonId = null;
    $scope.selectResponsible = function (e) {
        $scope.ResponsiblePerson = e.data.EmployeeName;
        $scope.ResponsiblePersonId = e.data.SystemId;
        $scope.closeEmpPopUp();
    }

    $scope.StorageLocation = null;
    $scope.selectStorageLocation = function (e) {
        $scope.StorageLocation = e.data.UserName;
        $scope.StorageLocationId = e.data.Id;
        $scope.closeStorageLocation();
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
    $scope.getResponsiblePerson();

    $scope.getResponsiblePersonId = function () {
        $http({
            method: 'POST',
            data: { 'ResponsiblePersonId': $scope.ResponsiblePersonId,},
            url: $scope.path + 'getResponsiblePersonId',
        }).then(function success(response) {
            $scope.ResponsiblePerson = JSON.stringify(response.data[0].ResponsiblePerson.replace(/\"/g, ""));
            $scope.ResponsiblePerson = $scope.ResponsiblePerson.replace(/\"/g, "");
            
        });
    }

    $scope.getStorageLocationId = function () {
        $http({
            method: 'POST',
            data: { 'StorageLocation': $scope.StorageLocationId, },
            url: $scope.path + 'getStorageLocationId',
        }).then(function success(response) {
            $scope.StorageLocation = JSON.stringify(response.data[0].StorageLocation);
            $scope.StorageLocation = $scope.StorageLocation.replace(/\"/g, "");
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
        $scope.ResponsiblePersonId = args.data.ResponsiblePersonId;
        $scope.StorageLocationId = args.data.StorageLocationId;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
            $scope.getStorageLocationId();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.path+'Save',
            data: {
                'datas': $scope.ModelNew,
                'StorageLocation': $scope.StorageLocationId,
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
                $scope.getData();

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
                    ClearFields();
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
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
       
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
          

            };
        $scope.StorageLocation = null;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
}