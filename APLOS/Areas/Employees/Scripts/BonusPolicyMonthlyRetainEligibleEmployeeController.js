'use strict';
BonusPolicyMonthlyRetainEligibleEmployeeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function BonusPolicyMonthlyRetainEligibleEmployeeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.pFEmployeeAppliedList = [];
    $scope.path = 'Employees/BonusPolicyMonthlyRetainEligibleEmployee/GetList';
    $scope.selectedEntity = null;
    $scope.getBonusPolicyMonthlyRetainEligibleEmployeeSavedList = function () {
        $http({
            method: 'GET',
            url: 'Employees/BonusPolicyMonthlyRetainEligibleEmployee/GetList'
        }).then(function successCallback(response) {
            $scope.pFEmployeeAppliedList = response.data.Rows;
        });
    };

    //$scope.getBonusPolicyMonthlyRetainEligibleEmployeeSavedList();
    $scope.BonusPolicyMonthlyRetainEligibleEmployeeOb = {
        Id: null,
        PlantId: $window.plantId,
        EffectiveDate:null
    }
    $('.datepicker').datepicker({
        autoclose: true,
        minViewMode: 1,
        format: 'MM-yyyy'
    });
    //PFOptionalEmployee
    $scope.BonusPolicyMandatoryEmployeeList = [];

    $scope.BonusPolicyMandatoryEmployeeList = [];

    $scope.SearchByBonusPolicyMandatoryEmployeeList = [
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

    $scope.popUpBonusPolicyMandatoryEmployeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getBonusPolicyMandatoryEmployeeData = function (pageno) {
        baseService.paginationBase("Employees/BonusPolicyMonthlyRetainEligibleEmployee/QueryForMandatoryBonusEmployee?plantId=" + $scope.BonusPolicyMonthlyRetainEligibleEmployeeOb.PlantId, pageno, $scope.popUpBonusPolicyMandatoryEmployeeParameters)
            .then(function (result) {
                $scope.BonusPolicyMandatoryEmployeeList = result.Rows;

                $scope.popUpBonusPolicyMandatoryEmployeeParameters.total_count = result.Total - $scope.BonusPolicyMandatoryEmployeeList.length;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    }
    $scope.getBonusPolicyMandatoryEmployeeData();

    //PFOptionalEmployee
    $scope.BonusPolicyOptionalEmployeeList = [];
    $scope.searchByBonusPolicyOptionalEmployeeList = [
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

    $scope.popUpBonusPolicyOptionalEmployeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getBonusPolicyOptionalEmployeeData = function (pageno) {
        baseService.paginationBase("Employees/BonusPolicyMonthlyRetainEligibleEmployee/QueryForOptionalBonusEmployee?plantId=" + $scope.BonusPolicyMonthlyRetainEligibleEmployeeOb.PlantId, pageno, $scope.popUpBonusPolicyOptionalEmployeeParameters)
            .then(function (result) {
                $scope.BonusPolicyOptionalEmployeeList = result.Rows;
                for (var i = 0; i < $scope.BonusPolicyOptionalEmployeeList.length; i++) {
                    $scope.BonusPolicyOptionalEmployeeList[i].AddedDate = $filter('dateFiltering')($scope.BonusPolicyOptionalEmployeeList[i].AddedDate, 'dd-MMM-yyyy');
                    $scope.BonusPolicyOptionalEmployeeList[i].StartDate = $filter('dateFiltering')($scope.BonusPolicyOptionalEmployeeList[i].StartDate, 'dd-MMM-yyyy');
                    $scope.BonusPolicyOptionalEmployeeList[i].EndDate = $filter('dateFiltering')($scope.BonusPolicyOptionalEmployeeList[i].EndDate, 'dd-MMM-yyyy');
                }
                $scope.popUpBonusPolicyOptionalEmployeeParameters.total_count = result.Total - $scope.BonusPolicyOptionalEmployeeList.length;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getBonusPolicyOptionalEmployeeData();

    function getPFEligibleEmployeeSavedList() {
        $scope.BonusOptionalEmployeeSaveList = [];

        angular.forEach($scope.BonusPolicyOptionalEmployeeList, function (item) {
            item.IsApproved = true;
            if (item.IsActive === false) {
                if (baseService.isUndefinedOrNull(item.EndDate)) {
                    throw "Effective date require";
                }

                //var date = new Date(item.EndDate);
                //var lastDay = new Date(date.getFullYear(), date.getMonth(), 0);
                //item.EndDate = lastDay;
            } else {
                item.EndDate = null;
            }
            $scope.BonusOptionalEmployeeSaveList.push(item);
        });

    }

    $scope.Save = function () {
        try {
            getPFEligibleEmployeeSavedList();
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Employees/BonusPolicyMonthlyRetainEligibleEmployee/Edit',
                    data: { 'bonusPolicyMonthlyRetainEligibleEmployee': $scope.BonusOptionalEmployeeSaveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getDefaultDate = function (index,event) {
        if (event.currentTarget.checked === false) {
            //var d = new Date();
            //var l = new Date(d.getFullYear(), d.getMonth(), 0);
            //var f = new Date(d.getFullYear(), d.getMonth(), 1);
            //$scope.BonusPolicyOptionalEmployeeList[index].EndDate = $filter('dateFiltering')(l);
            //if (baseService.isUndefinedOrNull($scope.BonusPolicyOptionalEmployeeList[index].StartDate)) {
            //    $scope.BonusPolicyOptionalEmployeeList[index].StartDate = $filter('dateFiltering')(f);
            //}
            $scope.BonusPolicyOptionalEmployeeList[index].EndDate = null;
            if (baseService.isUndefinedOrNull($scope.BonusPolicyOptionalEmployeeList[index].StartDate)) {
                $scope.BonusPolicyOptionalEmployeeList[index].StartDate = null;
            }
        }
        //else {
        //    $scope.BonusPolicyOptionalEmployeeList[index].EndDate = null;
        //    if (baseService.isUndefinedOrNull($scope.BonusPolicyOptionalEmployeeList[index].StartDate)) {
        //        $scope.BonusPolicyOptionalEmployeeList[index].StartDate = null;
        //    }
        //}
       
    }



}