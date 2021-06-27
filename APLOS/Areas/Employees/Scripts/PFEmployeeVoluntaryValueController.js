'use strict';
PFEmployeeVoluntaryValueController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function PFEmployeeVoluntaryValueController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$window) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.pFEmployeeVoluntaryValueList = [];
    $scope.path = 'Employees/PFEmployeeVoluntaryValue/GetList';
    $scope.selectedEntity = null;
    //$scope.getPFEmployeeVoluntaryValueSavedList();
    $scope.PFEmployeeVoluntaryValueOb = {
        Id: null,
        PlantId: $window.plantId,
        EffectiveDate: null
    }
    //PFMandatoryEmployee
    $('.datepicker').datepicker({
        autoclose: true,
        minViewMode: 1,
        format: 'MM-yyyy'
    });
    $scope.searchBypFEmployeeVoluntaryValueList = [
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
        sort: 'EmployeeCode',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
   
    $scope.gettPFEmployeeVoluntaryValueData = function () {
        var effectiveDate = $scope.PFEmployeeVoluntaryValueOb.EffectiveDate !== "" ? $filter('dateFiltering')($scope.PFEmployeeVoluntaryValueOb.EffectiveDate, 'dd-MM-yyyy') : ""
        $scope.getPFEmpVoluntaryData = function (pageno) {
            baseService.paginationBase("Employees/PFEmployeeVoluntaryValue/QueryPFEmpVoluntaryValue?plantId=" + $scope.PFEmployeeVoluntaryValueOb.PlantId + '&effectiveDate=' + effectiveDate, pageno, $scope.popUpPFMandatoryEmployeeParametersC)
                .then(function (result) {
                    $scope.pFEmployeeVoluntaryValueList = result.Rows;
                    $scope.popUpPFMandatoryEmployeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getPFEmpVoluntaryData();
    };
    $scope.popUpPFMandatoryEmployeeParametersC = {
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
    $scope.gettPFEmployeeVoluntaryCheckedValueData = function () {
        var effectiveDate = $scope.PFEmployeeVoluntaryValueOb.EffectiveDate !== "" ? $filter('dateFiltering')($scope.PFEmployeeVoluntaryValueOb.EffectiveDate, 'dd-MM-yyyy') : ""
        $scope.getPFEmpVoluntaryData = function (pageno) {
            baseService.paginationBase("Employees/PFEmployeeVoluntaryValue/QueryPFEmpVoluntaryValueChecked?plantId=" + $scope.PFEmployeeVoluntaryValueOb.PlantId + '&effectiveDate=' + effectiveDate, pageno, $scope.popUpPFMandatoryEmployeeParameters)
                .then(function (result) {
                    $scope.pFEmployeeVoluntaryValueList = result.Rows;
                    $scope.popUpPFMandatoryEmployeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getPFEmpVoluntaryData();
    };
    $scope.gettPFEmployeeVoluntaryValueData();
    //

    //Save
    $scope.prePareField = function (flag, index) {
        if (flag === false) {
            $scope.pFEmployeeVoluntaryValueList[index].VoluntaryPFValue = null;
        }
    };
    function getPFEmployeeVoluntaryValueSavedList() {
        $scope.pFEmployeeVoluntaryValueSavedList = [];
        angular.forEach($scope.pFEmployeeVoluntaryValueList, function (item) {
            item.EffectiveDate = $scope.PFEmployeeVoluntaryValueOb.EffectiveDate;
            if (item.Flag === false && item.VoluntaryPFValue !== null) {
                item.VoluntaryPFValue = null;
            }
            if (item.Flag && item.VoluntaryPFValue === null) {
                throw " Give value for " + item.EmployeeName;
            }
            if (item.Flag && item.VoluntaryPFValue !== null) {
                if (item.VoluntaryPFValue > item.EmpVolunValPer) {
                    throw item.EmployeeName + " Voluntary PF Contribution Value can not exceed " + item.EmpVolunValPer;
                }
                $scope.pFEmployeeVoluntaryValueSavedList.push(item);
            }
        });
    }
    $scope.Save = function () {
        try {
            if ($scope.PFEmployeeVoluntaryValueOb.EffectiveDate === "") {
                throw "please select effective date";
            }
            getPFEmployeeVoluntaryValueSavedList();
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Employees/PFEmployeeVoluntaryValue/create',
                    data: { 'pFEmployeeVoluntaryValue': $scope.pFEmployeeVoluntaryValueSavedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getPFEmployeeVoluntaryValueMasterOnEntityChange($scope.selectedEntity);
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //Deleting Rows from PFEmployeeVoluntaryValueList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.PFEmployeeVoluntaryValueId = data.Id;
        $scope.index = index;
        if (baseService.isUndefinedOrNull($scope.PFEmployeeVoluntaryValueId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.PFEmployeeVoluntaryValueId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeletePFEmployeeVoluntaryValueList = function () {
        $scope.pFEmployeeVoluntaryValueList.splice($scope.index, 1);
        $scope.id = null;
        $scope.index = null;
        $scope.PFEmployeeVoluntaryValueId = null;
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