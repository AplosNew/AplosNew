'use strict';
salarySlabWiseValueController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function salarySlabWiseValueController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Salary Slab Wise Value';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/SalarySlabWiseValue/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.SalarySlabWiseValueModel = {
        Id: null,
        ValueSalaryHeadId: null,
        ValueSalaryHeadAmount: null,
        BaseSalaryHeadId: null,
        BaseSalaryHeadMinAmount: null,
        BaseSalaryHeadMinAmount: null,
        BaseSalaryHeadMaxAmount: null,
        Active: false,         
    };

    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get('Attendances/SalarySlabWiseValue/GetSalaryHeadListeList')
            .then(function (response) {
                $scope.SalaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();

    $scope.SalarySlabWiseValueList = [];
    $scope.getListData = function () {
        $http.get('Attendances/SalarySlabWiseValue/getSalarySlabWiseValue')
            .then(
                function successCallback(response) {
                    $scope.SalarySlabWiseValueList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.SalarySlabWiseValueList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getListData();

    $scope.recorddoubleclick = function () {
        var gridObj = $("#salaryslabwisevalue").data("ejGrid");
        $scope.SalarySlabWiseValueModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';
        } catch (e) {
        }
        $scope.getListData();
    };

    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'SalarySlab': $scope.SalarySlabWiseValueModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SalarySlabWiseValueModel = {};
                    $scope.Clear();
                    $scope.getListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SalarySlabWiseValueModel.Id)) {
            $http.get('Attendances/SalarySlabWiseValue/Delete?Id=' + $scope.SalarySlabWiseValueModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.SalarySlabWiseValueModel = {};
                        $scope.Clear();
                        $scope.getListData();                        
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.SalarySlabWiseValueModel = {};
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Value Salary Head", $scope.SalarySlabWiseValueModel.ValueSalaryHeadId);         
            CheckField("Base Salary Head", $scope.SalarySlabWiseValueModel.BaseSalaryHeadId);         
            CheckField("Base Salary Head MinAmount", $scope.SalarySlabWiseValueModel.BaseSalaryHeadMinAmount);               
            CheckField("Base Salary Head Max Amount", $scope.SalarySlabWiseValueModel.BaseSalaryHeadMaxAmount);               
            CheckField("Value Salary Head Amount", $scope.SalarySlabWiseValueModel.ValueSalaryHeadAmount);               
        } catch (ex) {
            throw ex;
        }
    }

}