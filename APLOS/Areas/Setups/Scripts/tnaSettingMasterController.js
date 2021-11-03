'use strict';
tnaSettingMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function tnaSettingMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.tnaSettingMasterList = [];
    $scope.tnaSettingMasterToUserList = [];
    $scope.path = 'Setups/TnaSettingMaster/';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.tnaSettingMaster = {
        Id: null,
        PlantId: null,
        JobLocationId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null
    };

    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
        $scope.tnaSettingMasterList = [];
    });
    $scope.jobLocationList = [];
    $scope.getJobLocationOnPlantChange = function () {
        cboService.getJobLocationCbo($scope.tnaSettingMaster.PlantId, function (result) {
            $scope.jobLocationList = result;
        });
    };
    $scope.GetSequence = function () {
        $http.get('Setups/TnaSettingMaster/getautosequence?plantId=' + $scope.tnaSettingMaster.PlantId + '&joblocationId=' + $scope.tnaSettingMaster.JobLocationId)
            .then(function (response) {
                $scope.tnaSettingMaster.Sequence = response.data;
            });
    };

    $scope.GetList = function () {
        $http.get('Setups/TnaSettingMaster/GetList?plantId=' + $scope.tnaSettingMaster.PlantId)
            .then(function (response) {
                $scope.tnaSettingMasterList = response.data.Rows;
            });

    }

    //#region Employee
    $scope.employeeList = [];
    $scope.employeePopUpShow = function () {
        try {
            $scope.employeeParameters = {
                limit: 10,
                offset: 0,
                order: 'asc',
                sort: '',
                searchBy: '',
                pageSize: 10,
                total_count: 0,
                search: null,
                serverPagination: true
            };
            $scope.searchEmployeeByList = [
                {
                    name: 'Employee Code',
                    value: 'EmployeeCode'
                },
                {
                    name: 'Employee Name',
                    value: 'EmployeeName'
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
            ];

            $scope.popUpUrl = '';
            $scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist?plantId=' + $scope.tnaSettingMaster.PlantId;
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';

            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                        //getListForm($scope.employeeList);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeEmployeePopUp = function () {
        angular.forEach($scope.employeeList, function (item) {
            if (item.Flag) {
                item.EmployeeInformationId = item.SystemId;
                item.PlantId = $scope.tnaSettingMaster.PlantId;
                item.CompanyGroupId = window.companyGroupId;
                $scope.tnaSettingMasterList.push(item);
            }
        });
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    function checkExisting(id) {
        for (var i = 0; i < $scope.lineEmployeeAssigns.length; i++) {
            var ob = $scope.lineEmployeeAssigns[i];
            if (baseService.isUndefinedOrNull(ob.EmployeeId) && !baseService.isUndefinedOrNull(ob.TempEmployeeId)) {
                if (ob.TempEmployeeId === id) {
                    return true;
                }
            } else {
                if (ob.EmployeeId === id) {
                    return true;
                }
            }
        }
        return false;
    }

    //#end region
    //#region Employee To
    $scope.employeeToList = [];
    $scope.employeeToPopUpShow = function () {
        try {
            $scope.employeeToParameters = {
                limit: 10,
                offset: 0,
                order: 'asc',
                sort: '',
                searchBy: '',
                pageSize: 10,
                total_count: 0,
                search: null,
                serverPagination: true
            };
            $scope.searchEmployeeToByList = [
                {
                    name: 'Employee Code',
                    value: 'EmployeeCode'
                },
                {
                    name: 'Employee Name',
                    value: 'EmployeeName'
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
            ];

            $scope.popUpToUrl = '';
            $scope.employeeToParameters.sort = '';
            $scope.employeeToParameters.searchBy = '';
            $scope.popUpToTitle = 'Employee';
            $scope.popUpToUrl = 'employees/approvalconfiguration/getemployeedatalist?plantId=' + $scope.tnaSettingMaster.PlantId;
            $scope.employeeToParameters.sort = 'EmployeeCode';
            $scope.employeeToParameters.searchBy = 'EmployeeCode';

            $scope.getEmployeeToData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeToParameters)
                    .then(function (result) {
                        $scope.employeeToList = result.Rows;
                        $scope.employeeToParameters.total_count = result.Total;
                        //getListForm($scope.employeeList);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp');
                    }).finally(function () {
                    });
            };

            angular.element(document.querySelector('#employeeToPopUp')).modal('show');
            $scope.getEmployeeToData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closeEmployeeToPopUp = function () {
        angular.forEach($scope.employeeToList, function (item) {
            if (item.Flag) {
                item.EmployeeInformationId = item.SystemId;
                $scope.tnaSettingMasterToUserList.push(item);
            }
        });
        angular.element(document.querySelector('#employeeToPopUp')).modal('hide');
    };
    var selectedEmpOb;
    $scope.showToUserAddPopUp = function (index, data) {
        $scope.selectedEmpIndex = index;
        selectedEmpOb = data;
        angular.element(document.querySelector('#employeeToPopUpAdd')).modal('show');
    }
    //#end region
    $scope.getSavedData = function () {
        $scope.tnaSettingMasterSaveOb = {};
        $scope.tnaSettingDetailSaveList = [];
        var ob = angular.copy(selectedEmpOb);
        $scope.tnaSettingMasterSaveOb.Id = ob.Id;
        $scope.tnaSettingMasterSaveOb.CompanyGroupId = ob.CompanyGroupId;
        $scope.tnaSettingMasterSaveOb.PlantId = ob.PlantId;
        $scope.tnaSettingMasterSaveOb.EmployeeInformationId = ob.EmployeeInformationId;
        angular.forEach($scope.tnaSettingMasterToUserList, function (item) {
            item.TnaSettingMasterId = ob.Id;
            item.EmployeeInformationId = item.EmployeeInformationId;
            $scope.tnaSettingDetailSaveList.push(item);
        });
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.tnaSettingMasterForm.$valid) {
            if ($scope.Action === "Save") {
                $scope.getSavedData();
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'tnaSettingMaster': $scope.tnaSettingMasterSaveOb,
                        'tnaSettingDetails': $scope.tnaSettingDetailSaveList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.tnaSettingMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.tnaSettingMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.tnaSettingMasterList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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
        $scope.Action = "Save";
        $scope.tnaSettingMaster = { PlantId: $scope.tnaSettingMaster.PlantId };
    }
}