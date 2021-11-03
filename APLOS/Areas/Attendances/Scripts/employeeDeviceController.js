'use strict';
employeeDeviceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http'];
function employeeDeviceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http) {
    $rootScope.title = 'Employee Device';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.AccessControllerEmployeeTags = [];
    $scope.path = 'Attendances/accesscontrolleremployeetag/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'createemloyeedevice';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Plant', 'Plant');
    
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
        MachineIP: null,
        DeviceID:null
    };

    $scope.machineList = [];
    cboService.getCboMachine(function (result) {
        $scope.machineList = result;
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
        $scope.selectedDeviceList = [];
        try {
            baseService.paginationBase('Attendances/accesscontrolleremployeetag/getallemployee', pageno, $scope.EmployeePopUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.EmployeePopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.selectEmployee = function (dr) {
        $scope.AccessControllerEmployeeTagNew.EmpInfoSystemID = dr.SystemId;
        $scope.AccessControllerEmployeeTagNew.EmployeeCode = dr.EmployeeCode;
        $scope.AccessControllerEmployeeTagNew.EmployeeName = dr.EmployeeName;
        angular.element(document.querySelector('#popUp')).modal('hide');
        $scope.getDeviceList(dr.SystemId);
    };
   
    $scope.ClearEmployee = function () {
        $scope.AccessControllerEmployeeTagNew.EmployeeCode = null;
        $scope.AccessControllerEmployeeTagNew.EmployeeName = null;
        $scope.selectedDeviceList = [];
        $scope.RegisterProximate = false;
        $scope.RegisterFP = false;
    }

    $scope.tempListnew = [];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            for (var i = 0; i < $scope.selectedDeviceList.length; i++) {
                
                    $scope.tempListnew.push({
                        Id: null,
                        EmpInfoSystemID: $scope.selectedDeviceList[i].EmpInfoSystemID,
                        GroupID: $scope.selectedDeviceList[i].GroupID,
                        PlantID: $scope.selectedDeviceList[i].PlantID
                    });
                
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
                            'AccessControllerEmployeeTags': $scope.tempListnew
                            , 'registerProximate': $scope.RegisterProximate, 'registerFP': $scope.RegisterFP, 'deviceId': $scope.AccessControllerEmployeeTag.DeviceID
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            $scope.tempListnew = [];
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.tempListnew = [];
                            $scope.getDeviceList();
                            $scope.AccessControllerEmployeeTag.DeviceID = null;
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

    $scope.getDeviceList = function () {
        $scope.selectedDeviceList = [];
        $http({
            method: 'GET',
            url: 'Attendances/accesscontrolleremployeetag/getemployeedeviceslist?deviceId=' + $scope.AccessControllerEmployeeTag.DeviceSystemID
        }).then(function successCallback(response) {
            $scope.selectedDeviceList = response.data;
            if ($scope.selectedDeviceList.length > 0) {
                $scope.RegisterProximate = $scope.selectedDeviceList[0].RegisterProximate;
                $scope.RegisterFP = $scope.selectedDeviceList[0].RegisterFP;
            }
        });
    };

    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.selectedDeviceList, data.SystemId) === false) {
                    $scope.selectedDeviceList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.selectedDeviceList.length; i++) {
                    if ($scope.selectedDeviceList[i].EmpInfoSystemID === data.SystemId) {
                        $scope.selectedDeviceList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, SystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpInfoSystemID === SystemId) {
                return true;
            }
        }
        return false;
    }

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            $scope.popUpDataList[i].Active = _isselected;
        }

        for (var j = 0; j < baseService.arrayLength($scope.popUpDataList); j++) {
            if (_isselected)
                $scope.selectedDeviceList.push($scope.popUpDataList[j]);
            else
                for (var k = 0; k < $scope.selectedDeviceList.length; k++) {
                    if ($scope.selectedDeviceList[k].EmpInfoSystemID === $scope.popUpDataList[j].SystemId) {
                        $scope.selectedDeviceList.splice(k, 1);
                        break;
                    }
                }
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpInfoSystemID === id) {
                return true;
            }
        }
        return false;
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.AccessControllerEmployeeTag = {};
        $scope.tempList = [];
        $scope.employeeInformation = {};
        $scope.selectedDeviceList = [];
        $scope.RegisterProximate = false;
        $scope.RegisterFP = false;
        $scope.AccessControllerEmployeeTag.DeviceID= null;
    }


    $scope.confirmDelete = function (Id, EmployeeCode, index) {
        $scope.index = index;
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + EmployeeCode + "]? ";
    };

    $scope.DeleteDetail = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.selectedDeviceList.splice($scope.index, 1);
            $scope.index = -1;
        } else {
            $http({
                method: 'POST',
                url: 'attendances/accesscontrolleremployeetag/delete',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDeviceList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };
}