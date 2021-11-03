'use strict';
maternityBenefitAfterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function maternityBenefitAfterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Maternity Benefit Disbursement';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/MaternityBenefitAfter/';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.getETListUrl = $scope.path + 'GetEmploymentTypelist';
    $scope.getDataForEditUrl = $scope.path + 'GetDataForEdit';
    $scope.getUrl = $scope.path + 'get';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.master = {
        Id: null,
        LeaveTransactionId: null,
        EmpSystemId: null,
        WageRate: 0,
        LeaveDays: 0,
        BeforeAmount: 0,
        AfterAmount: 0,
        BeforePaymentDate: null,
        AfterPaymentDate: null,
        IsPaidAfter: false,
        IsPaidBefore: false,
        AdditionalAmountBefore: 0,
        Deduction: 0,
        AdditionalAmount: 0,
        Remark: null,
        AdditionalAmountAfter: 0,
        IsPaidBefore: false,
        BeforePaymentDate: null,
        AfterPaymentDate: null
    };
    $scope.EmpSalaryInfo = [];

    $scope.ShowInfo = function () {
        try {
            var empid = $scope.EmployeeModel.EmpSystemId;
            var empLeaveId = $scope.EmployeeModel.LeaveTransactionId;
            var LeaveStartDate = $scope.EmployeeModel.LeaveStartDate;
            $http.get($scope.path + 'ShowInfo?empid=' + empid + '&LeaveTransactionId=' + empLeaveId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmpSalaryInfo = response.data;
                        //$scope.EmpSalaryInfoShow = false;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.LevalValue = function () {
        try {
            var empid = $scope.EmployeeModel.EmpSystemId;
            var empLeaveId = $scope.EmployeeModel.LeaveTransactionId;

            $http.get($scope.path + 'LevalValue?empid=' + empid + '&LeaveTransactionId=' + empLeaveId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeLeavelInFo = response.data;
                        $scope.EmployeeModel.WorkingDays = response.data[0].WorkingDays;
                        $scope.EmployeeModel.TotalWorkingDays = response.data[0].TotalWorkingDays;
                        $scope.EmployeeModel.AdditionalAmount = response.data[0].AdditionalAmount;
                        $scope.EmployeeModel.Remark = response.data[0].Remark;
                        $scope.EmployeeModel.TotalPayable = response.data[0].TotalPayable;
                        $scope.EmployeeModel.WageRate = response.data[0].WageRate;
                        $scope.EmployeeModel.AdditionalAmountBefore = response.data[0].AdditionalAmountBefore;
                        $scope.EmployeeModel.AdditionalAmountAfter = response.data[0].AdditionalAmountAfter;
                        $scope.EmployeeModel.BeforeAmount = response.data[0].BeforeAmount;
                        $scope.EmployeeModel.AfterAmount = response.data[0].AfterAmount;
                        $scope.EmployeeModel.Id = response.data[0].Id;
                        $scope.EmployeeModel.IsPaidAfter = response.data[0].IsPaidAfter;
                        $scope.EmployeeModel.IsPaidBefore = response.data[0].IsPaidBefore;
                        $scope.EmployeeModel.BeforePaymentDate = response.data[0].BeforePaymentDate;
                        $scope.EmployeeModel.AfterPaymentDate = response.data[0].AfterPaymentDate;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();

            $http.get($scope.getEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeInformationList = response.data;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EmployeeModel = {};
    $scope.SelectEmployee = function () {
        try {
            var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            $scope.EmployeeModel = gridObj.getSelectedRecords()[0];
            $scope.ShowInfo();
            $scope.LevalValue();
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    function GetMasterValue() {
        try {
            $scope.master.Id = '';
            for (var fn in $scope.master) {
                //if (fn !== 'Id') {
                $scope.master[fn] = $scope.EmployeeModel[fn];
                //}
            }
            console.log('master', $scope.master);
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            
            GetMasterValue();
            $http({
                method: 'POST',
                url: $scope.path + 'Save',
                data: { 'master': $scope.master },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                }

            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SeparationType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SeparationType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SeparationTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.EmpSalaryInfo = [];
        $scope.EmployeeModel = {};
    }

    //$scope.changeBefore = function () {
    //    $scope.master.FromDate = null;
    //}
    //$scope.changeAfter = function () {
    //    $scope.master.ToDate = null;
    //}
};