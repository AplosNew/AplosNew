'use strict';
PFEmployeeAppliedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function PFEmployeeAppliedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$window) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.pFEmployeeAppliedList = [];
    $scope.path = 'Employees/PFEmployeeApplied/GetList';
    $scope.selectedEntity = null;
    $scope.getPFEmployeeAppliedSavedList = function () {
        $http({
            method: 'GET',
            url: 'Employees/PFEmployeeApplied/GetList'
        }).then(function successCallback(response) {
            $scope.pFEmployeeAppliedList = response.data.Rows;
        });
    };

    //$scope.getPFEmployeeAppliedSavedList();
    $scope.PFEmployeeAppliedOb = {
        Id: null,
        PlantId: $window.plantId
    }
    //PFMandatoryEmployee
    $scope.PFMandatoryEmployeeList = [];
    $scope.searchByPFMandatoryEmployeeList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'EmpDesignation'
        }
        ,
        {
            'name': 'Department',
            'value': 'EMPDepartment'
        }
        ,
        {
            'name': 'Section',
            'value': 'EMPSection'
        }
        ,
        {
            'name': 'Sub Section',
            'value': 'EMPSubSection'
        }
    ];
    $scope.popUpPFMandatoryEmployeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeName',
        searchBy: 'EmployeeName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getPFMandatoryEmployeeData = function (pageno) {
        baseService.paginationBase("Employees/PFEmployeeApplied/QueryForPFMandatoryEmployee?plantId=" + $scope.PFEmployeeAppliedOb.PlantId, pageno, $scope.popUpPFMandatoryEmployeeParameters)
            .then(function (result) {
                $scope.PFMandatoryEmployeeList = result.Rows;
                $scope.popUpPFMandatoryEmployeeParameters.total_count = result.Total - $scope.pFEmployeeAppliedList.length;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getPFMandatoryEmployeeData();
    //
    //PFOptionalEmployee
    $scope.PFOptionalEmployeeList = [];
    $scope.searchByPFOptionalEmployeeList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'EmpDesignation'
        }
        ,
        {
            'name': 'Department',
            'value': 'EMPDepartment'
        }
        ,
        {
            'name': 'Section',
            'value': 'EMPSection'
        }
        ,
        {
            'name': 'Sub Section',
            'value': 'EMPSubSection'
        }
    ];
    $scope.popUpPFOptionalEmployeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeName',
        searchBy: 'EmployeeName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getPFOptionalEmployeeData = function (pageno) {
        baseService.paginationBase("Employees/PFEmployeeApplied/QueryForPFOptionalEmployee?plantId=" + $scope.PFEmployeeAppliedOb.PlantId, pageno, $scope.popUpPFOptionalEmployeeParameters)
            .then(function (result) {
                angular.forEach(result.Rows, function (item) {
                    if (item.Checked) {
                        item.IsPFNotEntitleGetAllownceKeepChecked = false;
                        item.PFNotEntitleGetAllownceCheckDisabled = true;
                    } else {
                        if (item.IsPFNotEntitleGetAllownce) {
                            item.IsPFNotEntitleGetAllownceKeepChecked = true;
                        } else {
                            item.IsPFNotEntitleGetAllownceKeepChecked = false;
                        }
                        //item.PFNotEntitleGetAllownceCheckDisabled = true;
                    }
                });
                $scope.PFOptionalEmployeeList = result.Rows;
                $scope.popUpPFOptionalEmployeeParameters.total_count = result.Total - $scope.PFOptionalEmployeeList.length;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getPFOptionalEmployeeData();
    //

    $scope.getNotEntitleGetAllownceChecked = function (index, data) {
        if (data.Checked === false) {
            if (data.IsNotEntGetEmplrAlwnDetail === true) {
                $scope.PFOptionalEmployeeList[index].IsPFNotEntitleGetAllownceKeepChecked = true;
                if (data.IsIndividualAlwnDetail) {
                    $scope.PFOptionalEmployeeList[index].PFNotEntitleGetAllownceCheckDisabled = false;
                } else {
                    $scope.PFOptionalEmployeeList[index].PFNotEntitleGetAllownceCheckDisabled = true;
                }
            }
        } else {
            $scope.PFOptionalEmployeeList[index].IsPFNotEntitleGetAllownceKeepChecked = false;
            $scope.PFOptionalEmployeeList[index].PFNotEntitleGetAllownceCheckDisabled = true;

            //if (data.IsIndividualAlwnDetail) {
            //    $scope.PFOptionalEmployeeList[index].PFNotEntitleGetAllownceCheckDisabled = false;
            //} else {
            //    $scope.PFOptionalEmployeeList[index].PFNotEntitleGetAllownceCheckDisabled = true;
            //}
        }
    };
    //PFEmployeeAppliedList for modal
    $scope.employeeInformationList = [];
    $scope.ShowEmployeeListPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.PFEmployeeAppliedOb.PlantId)) {
            throw ShowResult("Please select plant", 'failure');
        }
        $scope.searchByList = [
            {
                'name': 'Employee Code',
                'value': 'EmployeeCode'
            },
            {
                'name': 'Employee Name',
                'value': 'EmployeeName'
            },
            {
                'name': 'Department',
                'value': 'Department'
            }
        ];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeName',
            searchBy: 'EmployeeName',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.getData = function (pageno) {
            baseService.paginationBase("Employees/EmployeeInformation/GetEmployeeListWithPlant?plantId=" + $scope.PFEmployeeAppliedOb.PlantId, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.employeeInformationList = [];
                    angular.forEach(result.Rows, function (item) {
                        if (getExistEmployee($scope.pFEmployeeAppliedList, item.SystemId) === false) {
                            $scope.employeeInformationList.push(item);
                        }
                    })
                    $scope.popUpParameters.total_count = result.Total - $scope.pFEmployeeAppliedList.length;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeeInformaitonPopUp')).modal('show');
        $scope.getData();
    };
    function getExistEmployee(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
                break;
            }
        }
        return false;
    }
    //End PFEmployeeAppliedList for modal
    //Passing Data For PFEmployeeApplied List
    $scope.employeeInformationCloseListPopUp = function () {
        angular.forEach($scope.employeeInformationList, function (item) {
            if (item.Flag) {
                $scope.pFEmployeeAppliedList.push(
                    {
                        Id: null,
                        EmpSystemId: item.SystemId,
                        EmployeeName: item.EmployeeName,
                        Department: item.Department,
                        Designation: item.Designation,
                        IsEligible: false,
                        Flag: item.Flag
                    }
                );
            }
        });
        angular.element(document.querySelector('#employeeInformaitonPopUp')).modal('hide');
    };
    //Save
    $scope.hasDuplicate = function (list) {
        for (var i = 0; i < list.length; i++) {
            for (var x = i + 1; x < list.length; x++) {
                if (list[i].EmpSystemId == list[x].EmpSystemId) {
                    throw list[i].EmployeeName + " has duplicate row";
                }
            }
        }
    };
    function getPFEligibleEmployeeSavedList() {
        $scope.PFOptionalEmployeeSaveList = [];
        angular.forEach($scope.PFOptionalEmployeeList, function (item) {
            item.IsApproved = true;
            item.IsActive = item.Checked;
            item.IsNotEntGetEmplrAlwn = item.IsActive === false && (item.IsNotEntGetEmplrAlwnDetail === true && item.IsPFNotEntitleGetAllownceKeepChecked === true) ? true : false;
            //item.IsNotEntGetEmplrAlwn = item.IsActive === false && item.IsNotEntGetEmplrAlwnDetail === true ? true : false;
            //item.IsIndividualAlwn = item.IsNotEntGetEmplrAlwn === true ? true : false;
            item.AlwnSlrHd = item.IsNotEntGetEmplrAlwn === true ? item.AlwnSlrHdDetail : null;
            $scope.PFOptionalEmployeeSaveList.push(item);
        });
    }
    $scope.Save = function () {
        try {
            //$scope.hasDuplicate($scope.pFEmployeeAppliedList);
            getPFEligibleEmployeeSavedList();
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Employees/PFEmployeeApplied/create',
                    data: {'pFEligibleEmployee': $scope.PFOptionalEmployeeSaveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getPFEmployeeAppliedMasterOnEntityChange($scope.selectedEntity);
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //Deleting Rows from PFEmployeeAppliedList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.PFEmployeeAppliedId = data.Id;
        $scope.index = index;
        if (baseService.isUndefinedOrNull($scope.PFEmployeeAppliedId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.PFEmployeeAppliedId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeletePFEmployeeAppliedList = function () {
        $scope.pFEmployeeAppliedList.splice($scope.index, 1);
        $scope.id = null;
        $scope.index = null;
        $scope.PFEmployeeAppliedId = null;
    };
    //
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}