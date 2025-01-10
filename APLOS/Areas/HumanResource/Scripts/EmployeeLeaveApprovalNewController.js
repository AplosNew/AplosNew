'use strict';
EmployeeLeaveApprovalNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EmployeeLeaveApprovalNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Leave Approval New';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/EmployeeLeaveApprovalNew/';
    $scope.getListUrl = $scope.path + 'GetGrdAvailedLvDetails';
    $scope.getlvBalanceUrl = $scope.path + 'GetEmpLeaveBalance';
    $scope.savelvApprovalUrl = $scope.path + 'SaveLeaveApproval';
    $scope.savelvRejectUrl = $scope.path + 'SaveLeaveReject';




    $scope.AvailedLvDetails = [];
    $scope.LeaveBalanceList = [];

    $scope.GetGrdAvailedLvDetails = function () {
        $scope.AvailedLvDetails = [];
        $http.get($scope.getListUrl)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.AvailedLvDetails = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetGrdAvailedLvDetails();



    $scope.SelectLvDetails = function () {


        var eDialog = $("#dialogLvDetails").data("ejDialog");
        eDialog.open();
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.LeaveBalanceList = [];

        const d = new Date(data.ToDate);
        let year = d.getFullYear();

        //$http.get($scope.getlvBalanceUrl + '?EmpsystemId=' + data.EmployeeID)
        //    .then(
        //        function successCallback(response) {
        //            if (baseService.arrayLength(response.data) > 0) {
        //                $scope.LeaveBalanceList  = response.data;
        //            }
        //        },
        //        function errorCallback(response) {
        //            ShowResult(response, 'failure');
        //        });



        //$http.get('HumanResource/LeaveApplicationNew/GetEmpLeaveBalance?EmpsystemId=' + data.EmployeeID + '&calanderYearId=')
        //    .then(function (response) {
        //        $scope.LeaveBalanceList = response.data;
        //    });

        $http.get('HumanResource/LeaveApplicationNew/GetEmpLeaveBalanceNew?EmpsystemId=' + data.EmployeeID + '&calanderYearId=' + year)
            .then(function (response) {
                $scope.LeaveBalanceList = response.data;
            });
    };
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {
                $scope.AvailedLvDetails[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };






    $scope.CancelationReason = null;

    $scope.SavelvApproval = function () {
        //$scope.AvailedLvDetails = [];
        try {
            var LvList = [];
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {

                if ($scope.AvailedLvDetails[i].CheckBoxSelect === true) {
                    LvList.push($scope.AvailedLvDetails[i]);
                }

            }
            if (LvList.length == 0) {
                throw "Please Select Employees Leave.";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'LeaveData': LvList },
                url: $scope.savelvApprovalUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, "success");
                    $scope.GetGrdAvailedLvDetails();


                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetdialogCancelationReason = function () {
        try {
            var LvList = [];
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {

                if ($scope.AvailedLvDetails[i].CheckBoxSelect === true) {
                    LvList.push($scope.AvailedLvDetails[i]);
                }

            }
            if (LvList.length == 0) {
                throw "Please Select Employees.";
            }
            var eDialog = $("#dialogCancelationReason").data("ejDialog");
            eDialog.open();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };
    $scope.Cancel = function () {

        var eDialog = $("#dialogCancelationReason").data("ejDialog");
        eDialog.close();
        $scope.CancelationReason = null;

    };

    $scope.Reject = function () {
        //$scope.AvailedLvDetails = [];
        try {

            if (baseService.isUndefinedOrNull($scope.CancelationReason)) {
                throw "Please Enter Cancelation Reason.";
            }
            var LvList = [];
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {

                if ($scope.AvailedLvDetails[i].CheckBoxSelect === true) {
                    LvList.push($scope.AvailedLvDetails[i]);
                }

            }
            if (LvList.length == 0) {
                throw "Please Select Employees.";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'LeaveData': LvList, 'CancelationReason': $scope.CancelationReason },
                url: $scope.savelvRejectUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, "success");
                    $scope.GetGrdAvailedLvDetails();
                    var eDialog = $("#dialogCancelationReason").data("ejDialog");
                    eDialog.close();
                    $scope.CancelationReason = null;


                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}