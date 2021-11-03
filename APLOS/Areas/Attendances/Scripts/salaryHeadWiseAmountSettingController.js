'use strict';
salaryHeadWiseAmountSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function salaryHeadWiseAmountSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Salary Head Wise Amount Setting';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/SalaryHeadWiseAmountSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.salaryHeadWiseAmountSettingModel = {
        Id: null,
        SalaryHeadId: null,
        AllowanceComponent: null,
        DurationType: null,         
    };

    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get('Attendances/SalaryHeadWiseAmountSetting/GetSalaryHeadListeList')
            .then(function (response) {
                $scope.SalaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();

    $scope.SalaryHeadWiseAmountSettingList = [];
    $scope.getListData = function () {
        $http.get('Attendances/SalaryHeadWiseAmountSetting/getSalaryHeadWiseAmountSettinglist')
            .then(
                function successCallback(response) {
                    $scope.SalaryHeadWiseAmountSettingList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.SalaryHeadWiseAmountSettingList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getListData();

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridSalaryHeadWiseAmountSetting").data("ejGrid");
        $scope.salaryHeadWiseAmountSettingModel = gridObj.getSelectedRecords()[0];
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
                data: { 'SalaryHeadWiseAmountSetting': $scope.salaryHeadWiseAmountSettingModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.salaryHeadWiseAmountSettingModel = {};
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
        if (!baseService.isUndefinedOrNull($scope.salaryHeadWiseAmountSettingModel.Id)) {
            $http.get('Attendances/SalaryHeadWiseAmountSetting/Delete?Id=' + $scope.salaryHeadWiseAmountSettingModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.salaryHeadWiseAmountSettingModel = {};
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
        $scope.salaryHeadWiseAmountSettingModel = {};
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
            CheckField("Salary Head", $scope.salaryHeadWiseAmountSettingModel.SalaryHeadId);         
            CheckField("Allowance Component", $scope.salaryHeadWiseAmountSettingModel.AllowanceComponent);         
            CheckField("Duration Type", $scope.salaryHeadWiseAmountSettingModel.DurationType);               
        } catch (ex) {
            throw ex;
        }
    }

}