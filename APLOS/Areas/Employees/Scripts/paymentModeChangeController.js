'use strict';
paymentModeChangeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function paymentModeChangeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Payment Mode Change';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Employees/PaymentModeChange/';
    $scope.LoadEmpListUrl = $scope.path + 'LoadEmployeelist';
    $scope.getPaymentModeUrl = $scope.path + 'GetPaymentMode';
    $scope.SaveChangeUrl = $scope.path + 'SaveChangeData';
 

    // load employee
    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.EmployeePaymentMode = null;
    $scope.EmployeeList = [];
    $scope.SelectedEmployeeList = [];
    $scope.getEmploymeeList = function () {
        
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Please Enter From.";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Please Enter to Date.";
            }
       
            var FDate = $filter('dateFiltering')($scope.FromDate, 'dd-M-yyyy');  
            var TDate = $filter('dateFiltering')($scope.ToDate, 'dd-M-yyyy');  
            $http.get($scope.LoadEmpListUrl + '?FromDate=' + FDate + '&ToDate=' + TDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeList = response.data;
                        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                        eDialog.open();
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //$scope.PaymentMode = [];
    //$scope.getPaymentModeList = function () {
    
    //    try {
    //        $http.get($scope.getPaymentModeUrl)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.Message, 'failure');
    //                }
    //                else {
    //                    $scope.PaymentMode = response.data;
    //                }
    //            },

    //                function errorCallBack(response) {
    //                    ShowResult(response.Message, 'failure');
    //                });


    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    $scope.GetSelectedEmployeeList = function () {
        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
        eDialog.close();
        try {
            $scope.SelectedEmployeeList = [];
            for (var i = 0; i < $scope.EmployeeList.length; i++) {

                if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                    $scope.SelectedEmployeeList.push($scope.EmployeeList[i]);
                }

            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;          

        }
       
        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.RemoveSelectedEmployeeList = function () {
       
        var gridObj = $("#GridSelectedEmployeeInfoList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        try {
           
            $scope.SelectedEmployeeList.splice($scope.SelectedEmployeeList.indexOf(data), 1);           
            gridObj.refreshContent();

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SaveChangeData = function () {

        try {
            var EmployeeSystemIdList = [];
            for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {               
                EmployeeSystemIdList.push($scope.SelectedEmployeeList[i].SystemId);              

            }
            if (EmployeeSystemIdList.length == 0) {
                throw "Please Select Employee.";
            }
            if (baseService.isUndefinedOrNull($scope.EmployeePaymentMode)) {
                throw "Please Select Employee Payment Mode.";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'EmployeeSystemIdList': EmployeeSystemIdList, 'EmployeePaymentMode': $scope.EmployeePaymentMode },
                url: $scope.SaveChangeUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
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
        $scope.EmployeePaymentMode = null;
        $scope.EmployeeList = [];
        $scope.SelectedEmployeeList = [];

        var gridObj = $("#GridSelectedEmployeeInfoList").data("ejGrid");
        gridObj.clearFiltering();
        var gridObj1 = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj1.clearFiltering();

    }
};