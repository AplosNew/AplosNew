'use strict';
accessControllerEmployeeTagController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http'];
function accessControllerEmployeeTagController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = 'Access Controller Employee';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.AccessControllerEmployeeTags = [];
    //$scope.selectedDeviceList = [];
    //$scope.dataListDevice = [];
    $scope.path = 'Attendances/accesscontrolleremployeetag/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Plant', 'Plant');

    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
    $scope.RegisterProximate = false;
    $scope.RegisterFP = false;
    $scope.AccessControllerEmployeeTag = {
        Id: null,
        EmpInfoSystemID: null,
        DeviceSystemID: null,
        RegisterStatus: null,
        GroupID: null,
        PlantID: null,
        EmployeeName: null,
        MachineID: null,
        MachineIP: null
    };

    $scope.AccessControllerEmployeeTagNew = Object.assign({}, $scope.AccessControllerEmployeeTag);

    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });

    $scope.EmployeePopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.searchingEmpList = [
        {
            'name': 'Employee Id',
            'value': 'EmployeeId'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Email',
            'value': 'EmailId'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Given Designation',
            'value': 'GivenDesignation'
        }];

    $scope.getEmployeeModal = function (pageno) {
        try {
            if (baseService.isUndefinedOrNull($scope.AccessControllerEmployeeTagNew.PlantID)) {
                throw "First Select a Plant.";
            }
            baseService.paginationBase('Attendances/accesscontrolleremployeetag/getallemployee?plantId=' + $scope.AccessControllerEmployeeTagNew.PlantID, pageno, $scope.EmployeePopUpParameters)
                .then(function (result) {
                    $scope.dataListEmployee = result.Rows;
                    $scope.EmployeePopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            angular.element(document.querySelector('#PopUpEmployee')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.selectEmployee = function (dr) {
        $scope.AccessControllerEmployeeTagNew.EmpInfoSystemID = dr.SystemId;
        $scope.AccessControllerEmployeeTagNew.EmployeeCode = dr.EmployeeCode;
        $scope.AccessControllerEmployeeTagNew.EmployeeName = dr.EmployeeName;
        angular.element(document.querySelector('#PopUpEmployee')).modal('hide');
        $scope.getDeviceList(dr.SystemId);
    };

    $scope.getDeviceList = function (systemId) {
        $scope.selectedDeviceList = [];
        $http({
            method: 'GET',
            url: 'Attendances/accesscontrolleremployeetag/getemployeerelateddevices?systemId=' + systemId
        }).then(function successCallback(response) {
            $scope.selectedDeviceList = response.data;
            if ($scope.selectedDeviceList.length > 0) {
                $scope.RegisterProximate = $scope.selectedDeviceList[0].RegisterProximate;
                $scope.RegisterFP = $scope.selectedDeviceList[0].RegisterFP;
            }
        });
    };

    // CLEAR functions START
    $scope.ClearEmployee = function () {
        $scope.AccessControllerEmployeeTagNew.EmployeeCode = null;
        $scope.AccessControllerEmployeeTagNew.EmployeeName = null;
        $scope.selectedDeviceList = [];
        $scope.RegisterProximate = false;
        $scope.RegisterFP = false;
    };

    $scope.tempList = [];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            for (var i = 0; i < $scope.selectedDeviceList.length; i++) {
                var flg = $scope.selectedDeviceList[i];
                if (flg.Flag) {
                    $scope.tempList.push({
                        Id: $scope.selectedDeviceList[i].Id,
                        PlantID: $scope.AccessControllerEmployeeTagNew.PlantID,
                        EmpInfoSystemID: $scope.AccessControllerEmployeeTagNew.EmpInfoSystemID,
                        DeviceSystemID: $scope.selectedDeviceList[i].DeviceSystemID
                    });
                }
            }
            if (baseService.isUndefinedOrNull($scope.RegisterProximate)) {
                $scope.RegisterProximate = false;
            }
            if (baseService.isUndefinedOrNull($scope.RegisterFP)) {
                $scope.RegisterFP = false;
            }
            if ($scope.AccessControllerEmployeeTagForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: {
                            'AccessControllerEmployeeTags': $scope.tempList, 'empId': $scope.AccessControllerEmployeeTagNew.EmpInfoSystemID
                            , 'registerProximate': $scope.RegisterProximate, 'registerFP': $scope.RegisterFP
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.tempList = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getDeviceList($scope.AccessControllerEmployeeTagNew.EmpInfoSystemID);
                            $scope.tempList = [];
                            $scope.selectedDeviceList = [];
                            Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.AccessControllerEmployeeTag = {};
        $scope.tempList = [];
        $scope.AccessControllerEmployeeTagNew = {};
        $scope.employeeInformation = {};
        $scope.selectedDeviceList = [];
        $scope.RegisterProximate = false;
        $scope.RegisterFP = false;
    }
    // CLEAR functions END
}