'use strict';
salaryHeadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function salaryHeadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Salary Head";
    $scope.Action = 'Save';
    $scope.payrollGroupMasters = [];
    $scope.path = 'Payrolls/salaryhead/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.salaryHead = {
        GroupID: $window.companyGroupId,
        SalaryHeadID: null,
        SalaryHead: null,
        Description: null,
        HeadType: null,
        HeadCategory: null,
        ExtDataUpload: false,
        Sequence: null,
        PartOfNetPay: true,
        IsRetained: false,
        IsApplicableInFinalSettlement: false,
        TransactionType: null
    };
    $scope.salaryHeadNew = Object.assign({}, $scope.salaryHead);

    $scope.headCategoryList = [];
    cboService.getSalaryHeadCategoryCbo(function (data) {
        $scope.headCategoryList = data;
    });

    $scope.CheckUnCheckIsRetained = function () {
            $scope.salaryHeadNew.IsRetained = false;
    }

    $scope.getAutoSequence = function () {
        $http.get("payrolls/salaryhead/GetAutoSequence")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryHeadNew.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.getAutoSequence();

    $scope.salaryHeadList = [];
    $scope.getSavedData = function () {
        $scope.salaryHeadList = [];
        $http.get("payrolls/salaryhead/getsalaryheadlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.salaryHeadList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    $scope.EditData = function (obj) {
        $scope.SalaryHeadCategory = null;
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.salaryHeadNew = data;
        $scope.Action = 'Update';
        $scope.getDisable();
    }


    $scope.disabledExternalDataUpload = false;
    $scope.disabledNetpay = false;
    $scope.disabledIsGrossComponent = false;
    $scope.getDisable = function () {
        $scope.HeadValidation();
        if (!baseService.isUndefinedOrNull($scope.salaryHeadNew.HeadCategory)) {
            if ($scope.salaryHeadNew.HeadCategory.toUpperCase() === 'GROSS' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'TOTAL GROSS' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'CTC' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'NET PAYABLE') {

                $scope.salaryHeadNew.PartOfNetPay = false;
                $scope.salaryHeadNew.IsGrossComponent = false;

                $scope.disabledExternalDataUpload = true;
                $scope.disabledNetpay = true;
                $scope.disabledIsGrossComponent = true;


            } else if ($scope.salaryHeadNew.HeadCategory.toUpperCase() === 'PF EMPLOYER CONTRIBUTION') {
                $scope.salaryHeadNew.PartOfNetPay = false;
                $scope.salaryHeadNew.IsGrossComponent = false;

                $scope.disabledExternalDataUpload = false;
                $scope.disabledNetpay = true;
                $scope.disabledIsGrossComponent = true;
            }
            else if ($scope.salaryHeadNew.HeadCategory.toUpperCase() === 'ATTENDANCE BONUS') {
                $scope.salaryHeadNew.PartOfNetPay = true;
                $scope.salaryHeadNew.IsGrossComponent = false;

                $scope.disabledExternalDataUpload = false;
                $scope.disabledNetpay = true;
                $scope.disabledIsGrossComponent = true;
            } else if ($scope.salaryHeadNew.HeadCategory.toUpperCase() === 'OVERTIME') {
                $scope.salaryHeadNew.PartOfNetPay = true;
                $scope.salaryHeadNew.IsGrossComponent = false;

                $scope.disabledExternalDataUpload = false;
                $scope.disabledNetpay = true;
                $scope.disabledIsGrossComponent = true;
            } else if ($scope.salaryHeadNew.HeadCategory.toUpperCase() === 'BASIC') {
                $scope.salaryHeadNew.PartOfNetPay = true;
                $scope.salaryHeadNew.IsGrossComponent = true;

                $scope.disabledExternalDataUpload = false;
                $scope.disabledNetpay = true;
                $scope.disabledIsGrossComponent = true;
            }




            else {
                $scope.HeadTypeChange();
                $scope.disabledExternalDataUpload = false;
            }
        }
        else {
            $scope.disabledNetpay = false;
            $scope.disabledExternalDataUpload = false;
            // $scope.salaryHeadNew.PartOfNetPay = true;

        }
    };

    $scope.HeadTypeChange = function () {
        $scope.HeadValidation();
        if (!baseService.isUndefinedOrNull($scope.salaryHeadNew.HeadType)) {
            if ($scope.salaryHeadNew.HeadType === 'Deduction') {
                $scope.disabledNetpay = true;

                $scope.disabledIsGrossComponent = true;
                $scope.salaryHeadNew.PartOfNetPay = true;
                $scope.salaryHeadNew.IsGrossComponent = false;
            } else {
                $scope.disabledNetpay = false;
                $scope.disabledIsGrossComponent = false;
            }
        }
        else {
            $scope.disabledNetpay = false;
            $scope.disabledIsGrossComponent = false;
        }
    }

    $scope.IsGrossComponentChange = function () {
        if (!baseService.isUndefinedOrNull($scope.salaryHeadNew.IsGrossComponent)) {
            if ($scope.salaryHeadNew.IsGrossComponent === true) {

                $scope.salaryHeadNew.PartOfNetPay = true;
            } else {
                //
            }
        }

    }


    $scope.HeadValidation = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.salaryHeadNew.HeadCategory)) {
                if ($scope.salaryHeadNew.HeadCategory.toUpperCase() === 'GROSS' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'TOTAL GROSS' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'CTC' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'NET PAYABLE' || $scope.salaryHeadNew.HeadCategory.toUpperCase() === 'BASIC') {
                    if (!baseService.isUndefinedOrNull($scope.salaryHeadNew.HeadType)) {
                        if ($scope.salaryHeadNew.HeadType === 'Deduction') {
                            $scope.salaryHeadNew.HeadCategory = null;
                            $scope.salaryHeadNew.HeadType = null;
                            throw "Please select valid date";
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.salaryHeadForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.salaryHeadNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.salaryHeadList = [];
                            $scope.getSavedData();
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        try {
            $http({
                method: 'POST',
                url: 'payrolls/salaryhead/Delete?id=' + $scope.salaryHeadNew.SalaryHeadID
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.salaryHeadList = [];
                    $scope.getSavedData();
                    $scope.Clear();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.salaryHead = {};
        $scope.salaryHeadNew = {};
        $scope.salaryHeadNew.PartOfNetPay = true;
        $scope.disabledNetpay = false;
        $scope.getAutoSequence();
        $scope.Action = 'Save';
    }

}






